using System.Collections;
using System.Text.Json;
using SteelSeriesAPI.Sonar.Interfaces;
using SteelSeriesAPI.Sonar.Enums;
using SteelSeriesAPI.Sonar.Exceptions;
using SteelSeriesAPI.Sonar.Models;

namespace SteelSeriesAPI.Sonar.Http;

public class SonarHttpProvider : ISonarDataProvider
{
    public bool GetAudienceMonitoringState()
    {
        JsonDocument streamMonitoring = new HttpFetcher().Provide("streamRedirections/isStreamMonitoringEnabled");

        return streamMonitoring.RootElement.GetBoolean();
    }
}