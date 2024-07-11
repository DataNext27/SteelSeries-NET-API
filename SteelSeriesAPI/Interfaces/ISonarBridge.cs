namespace SteelSeriesAPI.Interfaces;

public interface ISonarBridge : ISonarDataProvider, ISonarCommandHandler
{
    bool StartListener();

    void StopListener();
}