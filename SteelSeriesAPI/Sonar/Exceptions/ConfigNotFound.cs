namespace SteelSeriesAPI.Sonar.Exceptions;

public class ConfigNotFound : Exception
{
    public ConfigNotFound() : base("No audio configuration found.") { }
    public ConfigNotFound(string message) : base(message) { }
    public ConfigNotFound(string message, Exception innerException) : base(message, innerException) { }
}