namespace SteelSeriesAPI.Sonar.Exceptions;

public class SonarListenerNotConnected : Exception
{
    public SonarListenerNotConnected() : base("Listener need to be connected before listening") { }
    public SonarListenerNotConnected(string message) : base(message) { }
    public SonarListenerNotConnected(string message, Exception innerException) : base(message, innerException) { }
}