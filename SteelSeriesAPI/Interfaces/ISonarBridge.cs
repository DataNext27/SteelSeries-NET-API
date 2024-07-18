namespace SteelSeriesAPI.Interfaces;

public interface ISonarBridge : ISonarDataProvider, ISonarCommandHandler
{
    bool IsRunning { get; }
    
    bool StartListener();

    void StopListener();
}