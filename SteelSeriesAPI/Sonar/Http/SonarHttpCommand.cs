using System.Globalization;
using System.Text.Json;
using SteelSeriesAPI.Sonar.Interfaces;
using SteelSeriesAPI.Sonar.Enums;
using SteelSeriesAPI.Sonar.Exceptions;

namespace SteelSeriesAPI.Sonar.Http;

public class SonarHttpCommand : ISonarCommandHandler
{
    private readonly ISonarBridge _sonarBridge;
    
    public SonarHttpCommand(SonarBridge sonarBridge)
    {
        _sonarBridge = sonarBridge;
    }
    
    public void SetRedirectionState(bool newState, Channel channel, Mix mix)
    {
        new HttpFetcher().Put("streamRedirections/" + mix.ToDictKey() + "/redirections/" + channel.ToDictKey() +
                              "/isEnabled/" + newState);
    }

    public void SetAudienceMonitoringState(bool newState)
    {
        new HttpFetcher().Put("streamRedirections/isStreamMonitoringEnabled/" + newState);
    }
}