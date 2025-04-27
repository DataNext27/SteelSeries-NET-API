using SteelSeriesAPI.Sonar.Http;
using SteelSeriesAPI.Sonar.Interfaces.Managers;

using System.Text.Json;

namespace SteelSeriesAPI.Sonar.Managers;

internal class AudienceMonitoringManager : IAudienceMonitoringManager
{
    public bool GetState()
    {
        JsonDocument streamMonitoring = new Fetcher().Provide("streamRedirections/isStreamMonitoringEnabled");

        return streamMonitoring.RootElement.GetBoolean();
    }
    
    public void SetState(bool newState)
    {
        new Fetcher().Put("streamRedirections/isStreamMonitoringEnabled/" + newState);
    }
}