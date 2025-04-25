using SteelSeriesAPI.Sonar.Http;
using SteelSeriesAPI.Sonar.Interfaces.Managers;

using System.Text.Json;

namespace SteelSeriesAPI.Sonar.Managers;

public class AudienceMonitoringManager : IAudienceMonitoringManager
{
    public bool GetState()
    {
        JsonDocument streamMonitoring = new HttpFetcher().Provide("streamRedirections/isStreamMonitoringEnabled");

        return streamMonitoring.RootElement.GetBoolean();
    }
    
    public void SetState(bool newState)
    {
        new HttpFetcher().Put("streamRedirections/isStreamMonitoringEnabled/" + newState);
    }
}