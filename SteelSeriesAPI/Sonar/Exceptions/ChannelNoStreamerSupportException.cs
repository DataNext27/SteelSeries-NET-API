namespace SteelSeriesAPI.Sonar.Exceptions;

public class ChannelNoStreamerSupportException : Exception
{
    public ChannelNoStreamerSupportException() : base("Only the Mic Channel is supported in this case.") { }
    public ChannelNoStreamerSupportException(string message) : base(message) { }
    public ChannelNoStreamerSupportException(string message, Exception innerException) : base(message, innerException) { }
}