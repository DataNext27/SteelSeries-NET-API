using SteelSeriesAPI.Sonar.Exceptions;
using SteelSeriesAPI.Sonar.Enums;
using SteelSeriesAPI.Sonar.Http;

using System.Text.Json;
using SteelSeriesAPI.Sonar.Interfaces.Managers;

namespace SteelSeriesAPI.Sonar.Managers;

internal class MixManager : IMixManager
{
    public bool GetState(Channel channel, Mix mix)
    {
        JsonElement streamRedirections = new Fetcher().Provide("streamRedirections").RootElement;

        foreach (JsonElement element in streamRedirections.EnumerateArray())
        {
            if (element.GetProperty("streamRedirectionId").GetString() == mix.ToDictKey())
            {
                foreach (JsonElement status in element.GetProperty("status").EnumerateArray())
                {
                    if (status.GetProperty("role").GetString() == channel.ToDictKey())
                    {
                        return status.GetProperty("isEnabled").GetBoolean();
                    }
                }
        
                throw new ChannelNotFoundException();
            }
        }
        
        throw new MixNotFoundException();
    }

    public void SetState(bool newState,Channel channel, Mix mix)
    {
        new Fetcher().Put("streamRedirections/" + mix.ToDictKey() + "/redirections/" + channel.ToDictKey() + "/isEnabled/" + newState);
    }
    
    public void Activate(Channel channel, Mix mix)
    {
        new Fetcher().Put("streamRedirections/" + mix.ToDictKey() + "/redirections/" + channel.ToDictKey() + "/isEnabled/true");
    }

    public void Deactivate(Channel channel, Mix mix)
    {
        new Fetcher().Put("streamRedirections/" + mix.ToDictKey() + "/redirections/" + channel.ToDictKey() + "/isEnabled/false");
    }
}