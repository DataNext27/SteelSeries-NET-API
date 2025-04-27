namespace SteelSeriesAPI.Sonar.Exceptions;

public class RoutedProcessNotFoundException : Exception
{
    public RoutedProcessNotFoundException() : base("Could not find any routed process") { }
    public RoutedProcessNotFoundException(string message) : base(message) { }
    public RoutedProcessNotFoundException(string message, Exception innerException) : base(message, innerException) { }
}