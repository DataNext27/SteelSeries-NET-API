using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace SteelSeriesAPI.Core;

/// <summary>
/// Discovers the Sonar web server address by reading coreProps.json
/// and querying the GG /subApps endpoint.
/// </summary>
public sealed class ServerDiscovery
{
    private readonly HttpClient _ggClient;
    private readonly string _corePropsPath;
    private readonly ILogger _logger;

    /// <summary>Creates a new discovery service.</summary>
    /// <param name="logger">Optional logger for diagnostics. When null, the library stays silent.</param>
    /// <param name="corePropsPath">Overrides the default coreProps.json location. Mainly useful for testing.</param>
    public ServerDiscovery(ILogger? logger = null, string? corePropsPath = null)
    {
        _logger = logger ?? NullLogger.Instance;
        _corePropsPath = corePropsPath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "SteelSeries", "GG", "coreProps.json");

        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, _, _, _) =>
                message.RequestUri?.IsLoopback ?? false
        };
        _ggClient = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(5) };
    }

    /// <summary>Reads coreProps.json and returns the GG encrypted server address.</summary>
    public string GetGGAddress()
    {
        if (!File.Exists(_corePropsPath))
            throw new SteelSeriesNotFoundException(
                $"coreProps.json not found at '{_corePropsPath}'. Is SteelSeries GG installed and running?");

        using var coreProps = JsonDocument.Parse(File.ReadAllText(_corePropsPath));

        if (!coreProps.RootElement.TryGetProperty("ggEncryptedAddress", out var address))
            throw new DiscoveryException("Field 'ggEncryptedAddress' missing from coreProps.json.");

        return address.GetString()
            ?? throw new DiscoveryException("Field 'ggEncryptedAddress' is null in coreProps.json.");
    }

    /// <summary>Queries /subApps and returns the Sonar web server base address.</summary>
    public async Task<Uri> DiscoverSonarAddressAsync(CancellationToken ct = default)
    {
        string ggAddress = GetGGAddress();
        _logger.LogDebug("Querying subApps at {Address}", ggAddress);

        string json;
        try
        {
            json = await _ggClient.GetStringAsync($"https://{ggAddress}/subApps", ct);
        }
        catch (HttpRequestException ex)
        {
            throw new DiscoveryException(
                $"Could not reach the GG server at '{ggAddress}'. Is SteelSeries GG running?", ex);
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            throw new DiscoveryException(
                $"The GG server at '{ggAddress}' did not respond within {_ggClient.Timeout.TotalSeconds:0}s.", ex);
        }

        return ParseSonarAddress(json);
    }

    /// <summary>Extracts the Sonar address from a /subApps JSON payload.</summary>
    internal static Uri ParseSonarAddress(string subAppsJson)
    {
        using var doc = JsonDocument.Parse(subAppsJson);

        if (!doc.RootElement.TryGetProperty("subApps", out var subApps) ||
            subApps.ValueKind != JsonValueKind.Object ||
            !subApps.TryGetProperty("sonar", out var sonar) ||
            sonar.ValueKind != JsonValueKind.Object)
            throw new DiscoveryException("Sonar entry not found in /subApps response.");
        
        // Only the 2 fields we actually need. Everything else may change freely.
        bool isRunning = sonar.TryGetProperty("isRunning", out var running) &&
                         running.ValueKind == JsonValueKind.True;
        if (!isRunning)
            throw new SonarNotRunningException();

        string? address = null;
        if (sonar.TryGetProperty("metadata", out var meta) &&
            meta.ValueKind == JsonValueKind.Object &&
            meta.TryGetProperty("webServerAddress", out var addr) &&
            addr.ValueKind == JsonValueKind.String)
        {
            address = addr.GetString();
        }

        if (string.IsNullOrEmpty(address))
            throw new SonarNotRunningException(); // startup in progress: transient, resolves on its own

        return new Uri(address);
    }
}