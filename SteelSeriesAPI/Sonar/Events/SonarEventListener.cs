using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SteelSeriesAPI.Core;
using SteelSeriesAPI.Sonar.Enums;
using SteelSeriesAPI.Sonar.Managers;
using SteelSeriesAPI.Sonar.Models;

namespace SteelSeriesAPI.Sonar.Events;

/// <summary>
/// Listens to the Sonar WebSocket event stream (/sock) and raises typed .NET events.
/// Automatically reconnects (with backoff) when the connection drops, e.g. when GG restarts.
/// On every (re)connection, Sonar pushes its full current state, so subscribers resynchronize for free.
/// </summary>
/// <remarks>
/// Events are raised from a background thread. Volume changes made from the Sonar UI sliders are
/// NOT broadcast by Sonar (observed 2026-08-08): <see cref="VolumeDataReceived"/> only fires on
/// connection and after major state changes such as a mode switch.
/// </remarks>
public sealed class SonarEventListener : IDisposable
{
    private const string SocketPath = "/sock";

    private readonly SonarHttpClient _httpClient;
    private readonly ILogger _logger;
    private CancellationTokenSource? _cts;
    private Task? _runLoop;

    /// <summary>Raised when the connection to Sonar is established or re-established.</summary>
    public event EventHandler? Connected;

    /// <summary>Raised when the connection to Sonar is lost. The listener will keep trying to reconnect.</summary>
    public event EventHandler? Disconnected;

    /// <summary>Raised when Sonar broadcasts a chat mix change (slider, hardware wheel...).</summary>
    public event EventHandler<ChatMixSetting>? ChatMixChanged;

    /// <summary>Raised when Sonar pushes a full volume snapshot (on connection and after major changes).</summary>
    public event EventHandler<VolumeSnapshot>? VolumeDataReceived;

    /// <summary>Raised when redirections changed. Sonar sends no details: re-query if needed.</summary>
    public event EventHandler? RedirectionsInvalidated;

    /// <summary>Raised when the selected config changed. Re-query the configs route if needed.</summary>
    public event EventHandler? SelectedConfigChanged;

    /// <summary>Raised for any Sonar event not yet mapped to a typed event.</summary>
    public event EventHandler<SonarUnknownEvent>? UnknownEventReceived;
    
    /// <summary>
    /// When set before <see cref="Start"/>, the listener also polls the volume state at this
    /// interval and raises <see cref="VolumeChanged"/> on differences. Sonar does not broadcast
    /// volume slider changes over its WebSocket (observed 2026-08-08), so polling is the only
    /// admin-free way to detect them. Null (the default) disables polling.
    /// </summary>
    public TimeSpan? VolumePollingInterval { get; set; }

    /// <summary>Raised when polling detects a mixer mode change. Requires <see cref="VolumePollingInterval"/>.</summary>
    public event EventHandler<ModeChange>? ModeChanged;
    
    /// <summary>Raised when polling detects a volume or mute change. Requires <see cref="VolumePollingInterval"/>.</summary>
    public event EventHandler<VolumeChange>? VolumeChanged;

    private Task? _pollLoop;

    internal SonarEventListener(SonarHttpClient httpClient, ILogger? logger = null)
    {
        _httpClient = httpClient;
        _logger = logger ?? NullLogger.Instance;
    }

    /// <summary>Starts listening in the background. Safe to call once; use <see cref="StopAsync"/> to stop.</summary>
    public void Start()
    {
        if (_runLoop is not null)
            throw new InvalidOperationException("The event listener is already running.");

        _cts = new CancellationTokenSource();
        _runLoop = Task.Run(() => RunAsync(_cts.Token));
        
        if (VolumePollingInterval is { } interval)
            _pollLoop = Task.Run(() => RunPollingAsync(interval, _cts.Token));
    }

    /// <summary>Stops listening and waits for the background loop to complete.</summary>
    public async Task StopAsync()
    {
        if (_cts is null || _runLoop is null) return;

        await _cts.CancelAsync();
        try { await Task.WhenAll(_runLoop, _pollLoop ?? Task.CompletedTask); }
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
                Uri http = await _httpClient.GetServerAddressAsync(ct);
                Uri wsUri = new UriBuilder(http) { Scheme = "ws", Path = SocketPath }.Uri;

                using var ws = new ClientWebSocket();
                await ws.ConnectAsync(wsUri, ct);

                _logger.LogDebug("Connected to Sonar event stream at {Uri}", wsUri);
                backoff = TimeSpan.FromSeconds(1); // reset on success
                
                wasConnected = true;
                RaiseSafely(() => Connected?.Invoke(this, EventArgs.Empty));
                await ReceiveLoopAsync(ws, ct);
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

            try { await Task.Delay(backoff, ct); }
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
            var result = await ws.ReceiveAsync(buffer, ct);
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
                    RaiseSafely(() => RedirectionsInvalidated?.Invoke(this, EventArgs.Empty));
                    break;

                case SonarEventNames.SelectedConfigUpdated:
                    RaiseSafely(() => SelectedConfigChanged?.Invoke(this, EventArgs.Empty));
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

        string? state = data.ValueKind == JsonValueKind.Object &&
                        data.TryGetProperty("state", out var s) &&
                        s.ValueKind == JsonValueKind.String
            ? s.GetString() : null;

        return new ChatMixSetting(balance, state);
    }

    /// <summary>Parses a SONAR_EVENT_VOLUME_DATA payload. Same shape as GET volumeSettings/streamer/.</summary>
    internal static VolumeSnapshot ParseVolumeSnapshot(JsonElement data)
    {
        var channels = new Dictionary<Channel, ChannelVolumes>();

        if (data.ValueKind == JsonValueKind.Object &&
            data.TryGetProperty("masters", out var masters))
        {
            channels[Channel.Master] = ParseChannelVolumes(masters);
        }

        if (data.ValueKind == JsonValueKind.Object &&
            data.TryGetProperty("devices", out var devices) &&
            devices.ValueKind == JsonValueKind.Object)
        {
            foreach (var device in devices.EnumerateObject())
            {
                Channel? channel = ChannelExtensions.FromJsonKey(device.Name);
                if (channel is null) continue; // unknown channel added by a future GG update: skip, don't crash

                channels[channel.Value] = ParseChannelVolumes(device.Value);
            }
        }

        return new VolumeSnapshot(channels);
    }

    private static ChannelVolumes ParseChannelVolumes(JsonElement node)
    {
        VolumeSetting? classic = null, personal = null, stream = null;

        if (node.ValueKind == JsonValueKind.Object)
        {
            if (node.TryGetProperty("classic", out var c) && c.ValueKind == JsonValueKind.Object)
                classic = ParseSetting(c);

            if (node.TryGetProperty("stream", out var st) && st.ValueKind == JsonValueKind.Object)
            {
                if (st.TryGetProperty(Mix.Personal.ToJsonKey(), out var p) && p.ValueKind == JsonValueKind.Object)
                    personal = ParseSetting(p);
                if (st.TryGetProperty(Mix.Stream.ToJsonKey(), out var sm) && sm.ValueKind == JsonValueKind.Object)
                    stream = ParseSetting(sm);
            }
        }

        return new ChannelVolumes(classic, personal, stream);
    }

    private static VolumeSetting ParseSetting(JsonElement node)
    {
        double volume = node.TryGetProperty("volume", out var v) &&
                        v.ValueKind == JsonValueKind.Number ? v.GetDouble() : 0.0;
        bool muted = node.TryGetProperty("muted", out var m) &&
                     m.ValueKind == JsonValueKind.True;
        return new VolumeSetting(volume, muted);
    }
    
    /// <summary>
    /// Polls the mode and the matching volume route, raising granular events on differences.
    /// Each volumeSettings route only reliably reflects its own mode's values (observed
    /// 2026-08-08: the other mode's section returns stale data), hence the mode-aware routing.
    /// </summary>
    private async Task RunPollingAsync(TimeSpan interval, CancellationToken ct)
    {
        var modeManager = new ModeManager(_httpClient);
        VolumeSnapshot? baseline = null;
        Mode? baselineMode = null;

        while (!ct.IsCancellationRequested)
        {
            try { await Task.Delay(interval, ct); }
            catch (OperationCanceledException) { break; }

            try
            {
                Mode mode = await modeManager.GetAsync(ct);

                string route = mode == Mode.Streamer
                    ? SonarRoutes.StreamerVolumes
                    : SonarRoutes.ClassicVolumes;

                using var doc = await _httpClient.GetAsync(route, ct);
                var snapshot = ParseVolumeSnapshot(doc.RootElement);

                if (baselineMode is not null && baselineMode != mode)
                {
                    Mode previous = baselineMode.Value;
                    RaiseSafely(() => ModeChanged?.Invoke(this, new ModeChange(previous, mode)));
                }

                // Only diff against a baseline captured in the same mode: comparing across
                // modes would produce spurious events from the stale sections.
                if (baseline is not null && baselineMode == mode)
                {
                    foreach (VolumeChange change in Diff(baseline, snapshot, mode))
                        RaiseSafely(() => VolumeChanged?.Invoke(this, change));
                }

                baseline = snapshot;
                baselineMode = mode;
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested) { break; }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Volume polling tick failed");
            }
        }
    }

    /// <summary>
    /// Computes the volume changes between two snapshots, comparing only the values
    /// that are reliable in the given mode.
    /// </summary>
    internal static IEnumerable<VolumeChange> Diff(VolumeSnapshot previous, VolumeSnapshot current, Mode mode)
    {
        foreach ((Channel channel, ChannelVolumes cur) in current.Channels)
        {
            if (!previous.Channels.TryGetValue(channel, out var prev)) continue;

            if (mode == Mode.Classic)
            {
                if (Changed(prev.Classic, cur.Classic))
                    yield return new VolumeChange(channel, null, prev.Classic!, cur.Classic!);
            }
            else
            {
                if (Changed(prev.Personal, cur.Personal))
                    yield return new VolumeChange(channel, Mix.Personal, prev.Personal!, cur.Personal!);
                if (Changed(prev.Stream, cur.Stream))
                    yield return new VolumeChange(channel, Mix.Stream, prev.Stream!, cur.Stream!);
            }
        }

        static bool Changed(VolumeSetting? previous, VolumeSetting? current) =>
            previous is not null && current is not null && previous != current;
    }

    /// <summary>Stops the listener without waiting. Prefer <see cref="StopAsync"/> for a graceful stop.</summary>
    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }
}