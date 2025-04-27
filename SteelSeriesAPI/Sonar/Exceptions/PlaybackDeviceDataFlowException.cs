namespace SteelSeriesAPI.Sonar.Exceptions;

public class PlaybackDeviceDataFlowException : Exception
{
    public PlaybackDeviceDataFlowException() : base("DataFlows do not match.") { }
    public PlaybackDeviceDataFlowException(string message) : base(message) { }
    public PlaybackDeviceDataFlowException(string message, Exception innerException) : base(message, innerException) { }
}