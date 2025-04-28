namespace SteelSeriesAPI.Sonar.Interfaces;

public interface ISonarSocket
{
    bool IsConnected { get; }

    bool Connect();
    
    bool Listen();

    void CloseSocket();
}