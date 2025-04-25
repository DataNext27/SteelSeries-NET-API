using SteelSeriesAPI.Sonar.Exceptions;
using SteelSeriesAPI.Sonar.Enums;
using SteelSeriesAPI.Sonar.Http;
using SteelSeriesAPI.Sonar.Interfaces.Managers;

using System.Text.Json;

namespace SteelSeriesAPI.Sonar.Managers;

public class RedirectionStateManager : IRedirectionStateManager
{
    public bool Get(Channel channel, Mix mix)
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
    
    public void Set(bool newState, Channel channel, Mix mix)
    {
        new HttpFetcher().Put("streamRedirections/" + mix.ToDictKey() + "/redirections/" + channel.ToDictKey() +
                              "/isEnabled/" + newState);
    }
}