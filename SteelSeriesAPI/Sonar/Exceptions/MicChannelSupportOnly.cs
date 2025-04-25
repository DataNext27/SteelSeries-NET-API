namespace SteelSeriesAPI.Sonar.Exceptions;

public class MicChannelSupportOnly : Exception
{
    public MicChannelSupportOnly() : base("Only the Mic Channel is supported in this case.") { }
    public MicChannelSupportOnly(string message) : base(message) { }
    public MicChannelSupportOnly(string message, Exception innerException) : base(message, innerException) { }
}