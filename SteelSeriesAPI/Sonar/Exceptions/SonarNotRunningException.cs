namespace SteelSeriesAPI.Sonar.Exceptions;

public class SonarNotRunningException : Exception
{
    public SonarNotRunningException() : base("Sonar is not running.") { }
    public SonarNotRunningException(string message) : base(message) { }
    public SonarNotRunningException(string message, Exception innerException) : base(message, innerException) { }
}