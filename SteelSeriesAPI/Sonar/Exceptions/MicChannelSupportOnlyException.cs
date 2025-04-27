namespace SteelSeriesAPI.Sonar.Exceptions;

public class MicChannelSupportOnlyException : Exception
{
    public MicChannelSupportOnlyException() : base("Only the Mic Channel is supported in this case.") { }
    public MicChannelSupportOnlyException(string message) : base(message) { }
    public MicChannelSupportOnlyException(string message, Exception innerException) : base(message, innerException) { }
}