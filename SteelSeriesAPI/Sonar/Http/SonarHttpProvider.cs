using System.Collections;
using System.Text.Json;
using SteelSeriesAPI.Sonar.Interfaces;
using SteelSeriesAPI.Sonar.Enums;
using SteelSeriesAPI.Sonar.Exceptions;
using SteelSeriesAPI.Sonar.Models;

namespace SteelSeriesAPI.Sonar.Http;

public class SonarHttpProvider : ISonarDataProvider
{
    public Mode GetMode()
    {
        string mode = new HttpFetcher().Provide("mode").RootElement.ToString();
        
        return (Mode)ModeExtensions.FromDictKey(mode, ModeMapChoice.StreamDict);
    }

    public bool GetRedirectionState(Channel channel, Mix mix)
    {
        if (channel == Channel.MASTER)
        {
            throw new MasterChannelNotSupportedException();
        }

        JsonDocument streamRedirections = new HttpFetcher().Provide("streamRedirections");
        JsonElement streamChannel = default;

        foreach (var element in streamRedirections.RootElement.EnumerateArray())
        {
            if (element.GetProperty("streamRedirectionId").GetString() == mix.ToDictKey())
            {
                streamChannel = element;
                break;
            }
        }

        JsonElement status = default;

        foreach (var element in streamChannel.GetProperty("status").EnumerateArray())
        {
            if (element.GetProperty("role").GetString() == channel.ToDictKey())
            {
                status = element;
                break;
            }
        }

        bool state = status.GetProperty("isEnabled").GetBoolean();

        return state;
    }

    public bool GetAudienceMonitoringState()
    {
        JsonDocument streamMonitoring = new HttpFetcher().Provide("streamRedirections/isStreamMonitoringEnabled");

        return streamMonitoring.RootElement.GetBoolean();
    }
}