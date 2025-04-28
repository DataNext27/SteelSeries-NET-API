namespace SteelSeriesAPI.Sonar.Exceptions;

public class MixNotFoundException : Exception
{
    public MixNotFoundException() : base("Mix could not be found.") { }
    public MixNotFoundException(string message) : base(message) { }
    public MixNotFoundException(string message, Exception innerException) : base(message, innerException) { }
}