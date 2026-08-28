using System.Text.Json;
using Microsoft.Extensions.Logging;
using SteelSeriesAPI.Core;
using SteelSeriesAPI.Sonar.Events;
using SteelSeriesAPI.Sonar.Managers;

namespace SteelSeriesAPI.Sonar;

/// <summary>
/// Entry point for controlling SteelSeries Sonar.
/// Create one instance and reuse it for the lifetime of your application.
/// </summary>
public sealed class SonarClient : IDisposable
{
    private readonly SonarHttpClient _httpClient;
    
    /// <summary>Real-time event stream from Sonar. Call <see cref="SonarEventListener.Start"/> to begin listening.</summary>
    public SonarEventListener Events { get; }

    /// <summary>Reads and switches the Sonar mixer mode.</summary>
    public IModeManager Mode { get; }

    /// <summary>Controls the volume and mute state of Sonar channels.</summary>
    public IVolumeSettingsManager VolumeSettings { get; }

    /// <summary>Reads and controls the chat mix (game/chat balance).</summary>
    public IChatMixManager ChatMix { get; }

    /// <summary>Controls audio redirections: device routing, mix toggles, and stream monitoring.</summary>
    public IRedirectionsManager Redirections { get; }

    /// <summary>Lists the audio devices known to Sonar, physical and virtual.</summary>
    public IAudioDeviceManager Devices { get; }

    /// <summary>Lists and selects Sonar audio configs (presets).</summary>
    public IConfigManager Configs { get; }

    /// <summary>Reads and controls which Sonar channel each application's audio is routed to.</summary>
    public IAppRoutingManager AppRouting { get; }
    
    /// <summary>Creates a new Sonar client.</summary>
    /// <param name="logger">Optional logger for diagnostics. When null, the library stays silent.</param>
    public SonarClient(ILogger? logger = null)
    {
        var discovery = new ServerDiscovery(logger);
        _httpClient = new SonarHttpClient(discovery, logger);
        
        Events = new SonarEventListener(_httpClient, logger);

        Mode = new ModeManager(_httpClient);
        VolumeSettings = new VolumeSettingsManager(_httpClient);
        ChatMix = new ChatMixManager(_httpClient);
        Redirections = new RedirectionsManager(_httpClient);
        Devices = new AudioDeviceManager(_httpClient);
        Configs = new ConfigManager(_httpClient);
        AppRouting = new AppRoutingManager(_httpClient);
    }
    
    /// <summary>
    /// Sends a GET request to an arbitrary Sonar route and returns the raw JSON response.
    /// Intended for exploration and debugging; prefer the typed managers for normal use.
    /// </summary>
    /// <param name="route">The route, relative to the Sonar server base address.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    public Task<JsonDocument> GetRawAsync(string route, CancellationToken ct = default) =>
        _httpClient.GetAsync(route, ct);
    
    /// <summary>
    /// Sends a PUT request to an arbitrary Sonar route.
    /// Intended for exploration and debugging; prefer the typed managers for normal use.
    /// </summary>
    /// <param name="route">The route, relative to the Sonar server base address.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    public Task PutRawAsync(string route, CancellationToken ct = default) =>
        _httpClient.PutAsync(route, ct);
    
    /// <summary>Resolves and returns the current Sonar web server address.</summary>
    /// <param name="ct">A token to cancel the operation.</param>
    public Task<Uri> GetServerAddressAsync(CancellationToken ct = default) =>
        _httpClient.GetServerAddressAsync(ct);

    /// <inheritdoc />
    public void Dispose()
    {
        Events.Dispose();
        _httpClient.Dispose();
    }
}