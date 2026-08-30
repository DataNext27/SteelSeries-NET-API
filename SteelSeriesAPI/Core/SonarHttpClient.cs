using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace SteelSeriesAPI.Core;

/// <summary>
/// Resilient HTTP client for the Sonar web server.
/// Caches the server address and transparently rediscovers it when GG restarts.
/// </summary>
public sealed class SonarHttpClient : IDisposable, ISonarTransport
{
    private readonly HttpClient _http;
    private readonly ServerDiscovery _discovery;
    private readonly SemaphoreSlim _discoveryLock = new(1, 1);
    private readonly ILogger _logger;

    private Uri? _baseAddress;

    /// <summary>Creates a new Sonar HTTP client.</summary>
    /// <param name="discovery">The discovery service used to locate the Sonar web server.</param>
    /// <param name="logger">Optional logger for diagnostics. When null, the library stays silent.</param>
    public SonarHttpClient(ServerDiscovery discovery, ILogger? logger = null)
    {
        _discovery = discovery;
        _logger = logger ?? NullLogger.Instance;
        _http = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
    }

    /// <summary>Sends a GET request to the Sonar server and returns the parsed JSON response.</summary>
    /// <param name="route">The route, relative to the Sonar server base address.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    public async Task<JsonDocument> GetAsync(string route, CancellationToken ct = default)
    {
        using var response = await SendAsync(HttpMethod.Get, route, ct).ConfigureAwait(false);
        var stream = await response.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
        await using (stream.ConfigureAwait(false))
        {
            return await JsonDocument.ParseAsync(stream, cancellationToken: ct).ConfigureAwait(false);
        }
    }


    /// <summary>Sends a PUT request to the Sonar server.</summary>
    /// <param name="route">The route, relative to the Sonar server base address.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    public async Task PutAsync(string route, CancellationToken ct = default)
    {
        using var _ = await SendAsync(HttpMethod.Put, route, ct).ConfigureAwait(false);
    }

    private async Task<HttpResponseMessage> SendAsync(
        HttpMethod method, string route, CancellationToken ct, bool isRetry = false)
    {
        Uri baseAddress = await GetBaseAddressAsync(ct).ConfigureAwait(false);
        var request = new HttpRequestMessage(method, new Uri(baseAddress, route));

        HttpResponseMessage response;
        try
        {
            response = await _http.SendAsync(request, ct).ConfigureAwait(false);
        }
        catch (HttpRequestException ex) when (!isRetry)
        {
            // Transport-level failure (connection refused, reset...):
            // GG may have restarted on a new port. Rediscover once, retry once.
            _logger.LogInformation(ex, "Request to {Route} failed, rediscovering Sonar address", route);
            InvalidateAddress();
            return await SendAsync(method, route, ct, isRetry: true).ConfigureAwait(false);
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested && !isRetry)
        {
            // Timeout (not a caller cancellation): treat as a transport failure.
            _logger.LogInformation(ex, "Request to {Route} timed out, rediscovering Sonar address", route);
            InvalidateAddress();
            return await SendAsync(method, route, ct, isRetry: true).ConfigureAwait(false);
        }

        if (!response.IsSuccessStatusCode)
        {
            string body = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

            if (body.Contains("Cannot be called in current mode", StringComparison.OrdinalIgnoreCase))
                throw new SonarWrongModeException(route);

            throw new SonarRequestException(route, (int)response.StatusCode, body);
        }

        return response;
    }

    private async Task<Uri> GetBaseAddressAsync(CancellationToken ct)
    {
        if (_baseAddress is not null) return _baseAddress;

        await _discoveryLock.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            // Another caller may have resolved it while we waited.
            return _baseAddress ??= await _discovery.DiscoverSonarAddressAsync(ct).ConfigureAwait(false);
        }
        finally
        {
            _discoveryLock.Release();
        }
    }
    
    /// <summary>
    /// Resolves and returns the current Sonar web server address,
    /// discovering it if not already cached.
    /// </summary>
    /// <param name="ct">A token to cancel the operation.</param>
    public Task<Uri> GetServerAddressAsync(CancellationToken ct = default) =>
        GetBaseAddressAsync(ct);

    internal void InvalidateAddress() => _baseAddress = null;

    /// <summary>Releases the underlying HTTP resources.</summary>
    public void Dispose()
    {
        _http.Dispose();
        _discoveryLock.Dispose();
    }
}