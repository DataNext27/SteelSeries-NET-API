namespace SteelSeriesAPI.Sonar.Exceptions;

public class MasterChannelNotSupported : Exception
{
    public MasterChannelNotSupported() : base("Master Channel is not supported in this case.") { }
    public MasterChannelNotSupported(string message) : base(message) { }
    public MasterChannelNotSupported(string message, Exception innerException) : base(message, innerException) { }
}