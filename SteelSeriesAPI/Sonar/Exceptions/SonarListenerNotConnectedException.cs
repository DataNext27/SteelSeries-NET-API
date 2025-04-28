namespace SteelSeriesAPI.Sonar.Exceptions;

public class SonarListenerNotConnectedException : Exception
{
    public SonarListenerNotConnectedException() : base("Listener need to be connected before listening") { }
    public SonarListenerNotConnectedException(string message) : base(message) { }
    public SonarListenerNotConnectedException(string message, Exception innerException) : base(message, innerException) { }
}