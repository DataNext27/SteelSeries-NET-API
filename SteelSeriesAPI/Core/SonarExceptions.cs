namespace SteelSeriesAPI.Core;

/// <summary>Base class for all exceptions thrown by this library.</summary>
public class SteelSeriesException : Exception
{
    /// <summary>Creates the exception with a message describing the failure.</summary>
    /// <param name="message">A description of what went wrong.</param>
    /// <param name="inner">The underlying exception, if any.</param>
    public SteelSeriesException(string message, Exception? inner = null)
        : base(message, inner) { }
}

/// <summary>SteelSeries GG is not installed or coreProps.json cannot be found.</summary>
public class SteelSeriesNotFoundException : SteelSeriesException
{
    /// <summary>Creates the exception with a message describing where the lookup failed.</summary>
    /// <param name="message">A description of what went wrong.</param>
    public SteelSeriesNotFoundException(string message) : base(message) { }
}

/// <summary>Sonar is not enabled or not running inside GG.</summary>
public class SonarNotRunningException : SteelSeriesException
{
    /// <summary>Creates the exception with a default message.</summary>
    public SonarNotRunningException()
        : base("Sonar is not running. Enable it in SteelSeries GG.") { }
}

/// <summary>The GG/Sonar API responded with an unexpected structure.</summary>
public class DiscoveryException : SteelSeriesException
{
    /// <summary>Creates the exception with a message describing where the discovery failed.</summary>
    /// <param name="message">A description of what went wrong.</param>
    /// /// <param name="inner">The underlying exception, if any.</param>
    public DiscoveryException(string message, Exception? inner = null)
        : base(message) { }
}

/// <summary>The Sonar API responded with an unexpected JSON structure.</summary>
public class SonarResponseException : SteelSeriesException
{
    /// <summary>Creates the exception with a message describing where the response failed.</summary>
    /// <param name="message">A description of what went wrong.</param>
    public SonarResponseException(string message) : base(message) { }
}

/// <summary>The Sonar server received the request but rejected it with an HTTP error status.</summary>
public class SonarRequestException : SteelSeriesException
{
    /// <summary>The HTTP status code returned by the server.</summary>
    public int StatusCode { get; }

    /// <summary>The raw response body, which may contain details about the rejection.</summary>
    public string? ResponseBody { get; }

    /// <summary>Creates the exception from the rejected route and the server response.</summary>
    /// <param name="route">The route that was rejected.</param>
    /// <param name="statusCode">The HTTP status code returned by the server.</param>
    /// <param name="responseBody">The raw response body, if any.</param>
    public SonarRequestException(string route, int statusCode, string? responseBody)
        : base($"Sonar rejected '{route}' with HTTP {statusCode}." +
               (string.IsNullOrWhiteSpace(responseBody) ? "" : $" Response: {responseBody}"))
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
    }
}

/// <summary>
/// The requested operation is not available in the current mixer mode.
/// For example, classic volume routes cannot be written while streamer mode is active.
/// Check <see cref="Sonar.Managers.IModeManager"/> to read or switch the mode.
/// </summary>
public class SonarWrongModeException : SteelSeriesException
{
    /// <summary>Creates the exception from the rejected route.</summary>
    /// <param name="route">The route that was rejected.</param>
    public SonarWrongModeException(string route)
        : base($"Sonar rejected '{route}': this operation is not available in the current mixer mode.") { }
}