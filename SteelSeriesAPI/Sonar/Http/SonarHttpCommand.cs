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
    
    public void SetMode(Mode mode)
    {
        new HttpFetcher().Put("mode/" + mode.ToDictKey(ModeMapChoice.StreamDict));
        Thread.Sleep(100); // Prevent bugs/freezes/crashes
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
    
    public void SetProcessToDeviceRouting(int pId, Channel channel)
    {
        if (channel == Channel.MASTER)
        {
            throw new MasterChannelNotSupported();
        }
        
        JsonDocument audioDeviceRouting = new HttpFetcher().Provide("AudioDeviceRouting");

        foreach (var element in audioDeviceRouting.RootElement.EnumerateArray())
        {
            if (element.GetProperty("role").GetString() == channel.ToDictKey())
            {
                if (channel == Channel.MIC)
                {
                    new HttpFetcher().Put("AudioDeviceRouting/capture/" + element.GetProperty("deviceId").GetString() + "/" + pId);
                    break;
                }
                else
                {
                    new HttpFetcher().Put("AudioDeviceRouting/render/" + element.GetProperty("deviceId").GetString() + "/" + pId);
                    break;
                }
            }
        }
    }
}