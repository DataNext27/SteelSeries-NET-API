using System.Text.Json;

namespace SteelSeriesAPI.Core;

/// <summary>Low-level transport to the Sonar web server.</summary>
public interface ISonarTransport
{
    /// <summary>Sends a GET request and returns the parsed JSON response.</summary>
    /// <param name="route">The route, relative to the Sonar server base address.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    Task<JsonDocument> GetAsync(string route, CancellationToken ct = default);

    /// <summary>Sends a PUT request.</summary>
    /// <param name="route">The route, relative to the Sonar server base address.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    Task PutAsync(string route, CancellationToken ct = default);
}