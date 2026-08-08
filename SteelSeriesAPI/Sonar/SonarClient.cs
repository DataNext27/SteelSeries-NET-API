using System.Text.Json;
using Microsoft.Extensions.Logging;
using SteelSeriesAPI.Core;
using SteelSeriesAPI.Sonar.Managers;

namespace SteelSeriesAPI.Sonar;

/// <summary>
/// Entry point for controlling SteelSeries Sonar.
/// Create one instance and reuse it for the lifetime of your application.
/// </summary>
public sealed class SonarClient : IDisposable
{
    private readonly SonarHttpClient _httpClient;
    
    /// <summary>Reads and switches the Sonar mixer mode.</summary>
    public IModeManager Mode { get; }

    /// <summary>Controls the volume and mute state of Sonar channels.</summary>
    public IVolumeSettingsManager VolumeSettings { get; }

    /// <summary>Creates a new Sonar client.</summary>
    /// <param name="logger">Optional logger for diagnostics. When null, the library stays silent.</param>
    public SonarClient(ILogger? logger = null)
    {
        var discovery = new ServerDiscovery(logger);
        _httpClient = new SonarHttpClient(discovery, logger);

        Mode = new ModeManager(_httpClient);
        VolumeSettings = new VolumeSettingsManager(_httpClient);
    }
    
    /// <summary>
    /// Sends a GET request to an arbitrary Sonar route and returns the raw JSON response.
    /// Intended for exploration and debugging; prefer the typed managers for normal use.
    /// </summary>
    /// <param name="route">The route, relative to the Sonar server base address.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    public Task<JsonDocument> GetRawAsync(string route, CancellationToken ct = default) =>
        _httpClient.GetAsync(route, ct);

    /// <inheritdoc />
    public void Dispose() => _httpClient.Dispose();
}