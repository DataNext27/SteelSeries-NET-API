namespace SteelSeriesAPI.Sonar.Exceptions;

public class PlaybackDeviceNotFoundException : Exception
{
    public PlaybackDeviceNotFoundException() : base("Could not find corresponding playback device") { }
    public PlaybackDeviceNotFoundException(string message) : base(message) { }
    public PlaybackDeviceNotFoundException(string message, Exception innerException) : base(message, innerException) { }
}