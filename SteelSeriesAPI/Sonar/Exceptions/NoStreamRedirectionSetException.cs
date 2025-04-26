namespace SteelSeriesAPI.Sonar.Exceptions;

public class NoStreamRedirectionSetException : Exception
{
    public NoStreamRedirectionSetException() : base("No redirection was set. Check the playback devices of the streamer mode") { }
    public NoStreamRedirectionSetException(string message) : base("No redirection was set. Check the playback devices of the streamer mode\n" + message) { }
    public NoStreamRedirectionSetException(string message, Exception innerException) : base("No redirection was set. Check the playback devices of the streamer mode\n" + message, innerException) { }
}