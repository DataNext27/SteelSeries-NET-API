namespace SteelSeriesAPI.Sonar.Interfaces;

public interface ISonarBridge : ISonarDataProvider, ISonarCommandHandler
{
    /// <summary>
    /// The running state of Sonar
    /// </summary>
    bool IsRunning { get; }
    
    /// <summary>
    /// Start listening to events happening on Sonar, such as changing volume...
    /// </summary>
    /// <returns>The state of the listener (false if it didn't start)</returns>
    bool StartListener();

    /// <summary>
    /// Stop the listener
    /// </summary>
    void StopListener();
}