namespace SteelSeriesAPI.Sonar.Exceptions;

public class MasterChannelNotSupportedException : Exception
{
    public MasterChannelNotSupportedException() : base("Master Channel is not supported in this case.") { }
    public MasterChannelNotSupportedException(string message) : base(message) { }
    public MasterChannelNotSupportedException(string message, Exception innerException) : base(message, innerException) { }
}