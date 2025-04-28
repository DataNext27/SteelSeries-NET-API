namespace SteelSeriesAPI.Sonar.Exceptions;

public class ChannelNotFoundException : Exception
{
    public ChannelNotFoundException() : base("Channel could not be found.") { }
    public ChannelNotFoundException(string message) : base(message) { }
    public ChannelNotFoundException(string message, Exception innerException) : base(message, innerException) { }
}