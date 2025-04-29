namespace SteelSeriesAPI.Sonar.Exceptions;

public class ConfigNotFoundException : Exception
{
    public ConfigNotFoundException() : base("No audio configuration found.") { }
    public ConfigNotFoundException(string message) : base(message) { }
    public ConfigNotFoundException(string message, Exception innerException) : base(message, innerException) { }
}