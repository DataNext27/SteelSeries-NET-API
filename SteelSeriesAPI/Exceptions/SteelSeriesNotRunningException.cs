namespace SteelSeriesAPI.Exceptions;

public class SteelSeriesNotRunningException : Exception
{
    public SteelSeriesNotRunningException() : base("SteelSeries is not running.") { }
    public SteelSeriesNotRunningException(string message) : base(message) { }
    public SteelSeriesNotRunningException(string message, Exception innerException) : base(message, innerException) { }
}