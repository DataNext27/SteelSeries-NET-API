using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SteelSeriesAPI.Core;
using SteelSeriesAPI.Sonar.Managers;
using SteelSeriesAPI.Sonar.Models;

namespace SteelSeriesAPI.Sonar.Events;

/// <summary>
/// Listens to the Sonar WebSocket event stream (/sock) and raises typed .NET events.
/// Automatically reconnects (with backoff) when the connection drops, e.g. when GG restarts.
/// On every (re)connection, Sonar pushes its full current state, so subscribers resynchronize for free.
/// </summary>
/// <remarks>
/// Events are raised from background threads. Each event is fed by one of three mechanisms,
/// invisible to subscribers:
/// <list type="bullet">
/// <item>WebSocket broadcast (real time): <see cref="ChatMixChanged"/>, <see cref="VolumeDataReceived"/>,
/// <see cref="AudioSessionOpened"/>, <see cref="AudioSessionClosed"/>, the *Invalidated signals,
/// and <see cref="Connected"/>/<see cref="Disconnected"/> (the connection itself).</item>
/// <item>Polling (requires <see cref="PollingInterval"/>): <see cref="VolumeChanged"/> and
/// <see cref="ModeChanged"/>. Sonar does not broadcast changes received through its own HTTP API,
/// such as UI slider moves (observed 2026-08-08), hence the polling.</item>
/// <item>Hybrid (WebSocket invalidation and polling, both feeding a fetch+diff):
/// <see cref="ClassicDeviceChanged"/>, <see cref="MixDeviceChanged"/>, <see cref="MixChannelToggled"/>,
/// <see cref="StreamMonitoringChanged"/> and <see cref="ConfigSelectionChanged"/>.</item>
/// </list>
/// </remarks>
public sealed partial class SonarEventListener : IDisposable
{
    private const string SocketPath = "/sock";

    private readonly SonarHttpClient _httpClient;
    private readonly ILogger _logger;
    private CancellationTokenSource? _cts;
    private CancellationToken _lifetime;
    private Task? _runLoop;
    private Task? _pollLoop;

    /// <summary>
    /// When set before <see cref="Start"/>, the listener periodically polls Sonar at this
    /// interval to detect changes that Sonar does not broadcast over its WebSocket:
    /// volume and mute levels, the mixer mode, redirection states (device routing,
    /// mix toggles, stream monitoring) and config selections. Smaller values reduce
    /// detection latency but increase local HTTP traffic; 300-500ms is a good balance
    /// for interactive use. Null (the default) disables polling: only WebSocket-broadcast
    /// events (chat mix, audio sessions, snapshots...) will be raised.
    /// </summary>
    public TimeSpan? PollingInterval { get; set; }

    /// <summary>Raised when the connection to Sonar is established or re-established.</summary>
    public event EventHandler? Connected;

    /// <summary>Raised when the connection to Sonar is lost. The listener will keep trying to reconnect.</summary>
    public event EventHandler? Disconnected;

    /// <summary>Raised when Sonar broadcasts a chat mix change (slider, hardware wheel...).</summary>
    public event EventHandler<ChatMixSetting>? ChatMixChanged;

    /// <summary>
    /// Raised when an application audio session appears on a device
    /// (app started playing, or was routed to this channel).
    /// </summary>
    public event EventHandler<DeviceRouting>? AudioSessionOpened;

    /// <summary>
    /// Raised when an application audio session leaves a device
    /// (app stopped playing, or was routed away from this channel).
    /// </summary>
    public event EventHandler<DeviceRouting>? AudioSessionClosed;

    /// <summary>
    /// Raised when Sonar signals that app routing changed, without details.
    /// Query <see cref="Managers.IAppRoutingManager.GetRoutingsAsync"/> for the new state,
    /// or rely on <see cref="AudioSessionOpened"/>/<see cref="AudioSessionClosed"/> which carry the data.
    /// </summary>
    public event EventHandler? RoutingInvalidated;

    /// <summary>Raised for any Sonar event not yet mapped to a typed event.</summary>
    public event EventHandler<SonarUnknownEvent>? UnknownEventReceived;

    internal SonarEventListener(SonarHttpClient httpClient, ILogger? logger = null)
    {
        _httpClient = httpClient;
        _logger = logger ?? NullLogger.Instance;

        _redirections = new RedirectionsManager(httpClient);
        _configs = new ConfigManager(httpClient);

        _redirectionsRefresher = new DebouncedRefresher("Redirections", RefreshRedirectionsAsync, _logger);
        _configsRefresher = new DebouncedRefresher("Configs", RefreshSelectedConfigsAsync, _logger);
    }

    /// <summary>
    /// Starts listening in the background. Call <see cref="StopAsync"/> to stop;
    /// the listener can then be started again.
    /// </summary>
    public void Start()
    {
        if (_runLoop is not null)
            throw new InvalidOperationException("The event listener is already running.");

        // Reset the diffing baselines: after a stop/start cycle, the world may have changed.
        _redirectionsBaseline = null;
        _selectedConfigsBaseline = null;

        _cts = new CancellationTokenSource();
        _lifetime = _cts.Token;
        _runLoop = Task.Run(() => RunAsync(_cts.Token));

        if (PollingInterval is { } interval)
            _pollLoop = Task.Run(() => RunPollingAsync(interval, _cts.Token));
    }

    /// <summary>Stops listening and waits for the background loops to complete.</summary>
    public async Task StopAsync()
    {
        if (_cts is null || _runLoop is null) return;

        await _cts.CancelAsync().ConfigureAwait(false);
        try { await Task.WhenAll(_runLoop, _pollLoop ?? Task.CompletedTask).ConfigureAwait(false); }
        catch (OperationCanceledException) { /* expected */ }

        _cts.Dispose();
        _cts = null;
        _runLoop = null;
        _pollLoop = null;
    }

    /// <summary>Connection lifecycle loop: connect, receive, and reconnect with backoff on failure.</summary>
    private async Task RunAsync(CancellationToken ct)
    {
        TimeSpan backoff = TimeSpan.FromSeconds(1);
        bool wasConnected = false;

        while (!ct.IsCancellationRequested)
        {
            try
            {
                Uri http = await _httpClient.GetServerAddressAsync(ct).ConfigureAwait(false);
                Uri wsUri = new UriBuilder(http) { Scheme = "ws", Path = SocketPath }.Uri;

                using var ws = new ClientWebSocket();
                await ws.ConnectAsync(wsUri, ct).ConfigureAwait(false);

                _logger.LogDebug("Connected to Sonar event stream at {Uri}", wsUri);
                backoff = TimeSpan.FromSeconds(1); // reset on success

                wasConnected = true;
                RaiseSafely(() => Connected?.Invoke(this, EventArgs.Empty));

                // Seed the diffing baselines right away, so the very first user change
                // after startup produces granular events instead of just creating the baseline.
                _redirectionsRefresher.Schedule(ct);
                _configsRefresher.Schedule(ct);

                await ReceiveLoopAsync(ws, ct).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Sonar event stream connection failed or dropped");
            }

            if (wasConnected)
            {
                wasConnected = false;
                RaiseSafely(() => Disconnected?.Invoke(this, EventArgs.Empty));
            }

            // GG may have restarted on a new port: force a fresh discovery on next attempt.
            _httpClient.InvalidateAddress();

            try { await Task.Delay(backoff, ct).ConfigureAwait(false); }
            catch (OperationCanceledException) { break; }

            backoff = TimeSpan.FromSeconds(Math.Min(backoff.TotalSeconds * 2, 30));
        }
    }

    /// <summary>Receives messages until the socket closes, reassembling fragmented frames.</summary>
    private async Task ReceiveLoopAsync(ClientWebSocket ws, CancellationToken ct)
    {
        var buffer = new byte[64 * 1024];
        using var message = new MemoryStream();

        while (ws.State == WebSocketState.Open && !ct.IsCancellationRequested)
        {
            var result = await ws.ReceiveAsync(buffer, ct).ConfigureAwait(false);
            if (result.MessageType == WebSocketMessageType.Close) return;

            message.Write(buffer, 0, result.Count);
            if (!result.EndOfMessage) continue;

            Dispatch(Encoding.UTF8.GetString(message.ToArray()));
            message.SetLength(0);
        }
    }

    /// <summary>Parses one raw message and raises the matching typed event.</summary>
    private void Dispatch(string json)
    {
        string? eventName = null;
        try
        {
            using var doc = JsonDocument.Parse(json);

            eventName = doc.RootElement.TryGetProperty("event", out var e) &&
                        e.ValueKind == JsonValueKind.String
                ? e.GetString()
                : null;

            JsonElement data = doc.RootElement.TryGetProperty("data", out var d) ? d : default;

            switch (eventName)
            {
                case SonarEventNames.ChatMixData:
                    RaiseSafely(() => ChatMixChanged?.Invoke(this, ParseChatMix(data)));
                    break;

                case SonarEventNames.VolumeData:
                    RaiseSafely(() => VolumeDataReceived?.Invoke(this, ParseVolumeSnapshot(data)));
                    break;

                case SonarEventNames.RedirectionStatusUpdate:
                case SonarEventNames.StreamMonitoringLockStatusUpdate:
                    RaiseSafely(() => RedirectionsInvalidated?.Invoke(this, EventArgs.Empty));
                    _redirectionsRefresher.Schedule(_lifetime);
                    break;

                case SonarEventNames.SelectedConfigUpdated:
                    RaiseSafely(() => ConfigsInvalidated?.Invoke(this, EventArgs.Empty));
                    _configsRefresher.Schedule(_lifetime);
                    break;

                case SonarEventNames.AudioSessionOpened:
                    if (AppRoutingManager.ParseRouting(data) is { } opened)
                        RaiseSafely(() => AudioSessionOpened?.Invoke(this, opened));
                    break;

                case SonarEventNames.AudioSessionClosed:
                    if (AppRoutingManager.ParseRouting(data) is { } closed)
                        RaiseSafely(() => AudioSessionClosed?.Invoke(this, closed));
                    break;

                case SonarEventNames.RoutingData:
                    RaiseSafely(() => RoutingInvalidated?.Invoke(this, EventArgs.Empty));
                    break;

                default:
                    RaiseSafely(() => UnknownEventReceived?.Invoke(this,
                        new SonarUnknownEvent(eventName ?? "", json)));
                    break;
            }
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Could not parse Sonar event message (event: {Event})", eventName);
        }
    }

    /// <summary>A subscriber throwing must never kill the receive loop.</summary>
    private void RaiseSafely(Action raise)
    {
        try { raise(); }
        catch (Exception ex) { _logger.LogWarning(ex, "A Sonar event subscriber threw an exception"); }
    }

    /// <summary>Parses an EVENT_SONAR_CHATMIX_DATA payload. Same shape as GET v1/chatMix.</summary>
    internal static ChatMixSetting ParseChatMix(JsonElement data)
    {
        double balance = data.ValueKind == JsonValueKind.Object &&
                         data.TryGetProperty("balance", out var b) &&
                         b.ValueKind == JsonValueKind.Number
            ? b.GetDouble() : 0.0;

        string? state = data.GetStringOrNull("state");

        return new ChatMixSetting(balance, state);
    }

    /// <summary>Stops the listener without waiting. Prefer <see cref="StopAsync"/> for a graceful stop.</summary>
    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _redirectionsRefresher.Dispose();
        _configsRefresher.Dispose();
    }
}