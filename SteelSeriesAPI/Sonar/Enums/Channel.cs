namespace SteelSeriesAPI.Sonar.Enums;

public enum Channel
{
    MASTER,
    GAME,
    CHAT,
    MEDIA,
    AUX,
    MIC
}

public enum ChannelMapChoice
{
    JsonDict,
    HttpDict,
    ChannelDict
}

public static class ChannelExtensions
{
    private static readonly Dictionary<Channel, string> ChannelJsonMap = new Dictionary<Channel, string>
    {
        { Channel.MASTER, "masters" },
        { Channel.GAME, "game" },
        { Channel.CHAT, "chatRender" }, 
        { Channel.MEDIA, "media" },
        { Channel.AUX, "aux" },
        { Channel.MIC, "chatCapture" } 
    };
    
    private static readonly Dictionary<Channel, string> ChannelHttpMap = new Dictionary<Channel, string>
    {
        { Channel.MASTER, "Master" },
        { Channel.GAME, "game" },
        { Channel.CHAT, "chatRender" }, 
        { Channel.MEDIA, "media" },
        { Channel.AUX, "aux" },
        { Channel.MIC, "chatCapture" } 
    };

    private static readonly Dictionary<Channel, string> ChannelMap = new Dictionary<Channel, string>
    {
        { Channel.MASTER, "master" },
        { Channel.GAME, "game" },
        { Channel.CHAT, "chat" }, 
        { Channel.MEDIA, "media" },
        { Channel.AUX, "aux" },
        { Channel.MIC, "mic" } 
    };
    
    public static string ToDictKey(this Channel channel, ChannelMapChoice context = ChannelMapChoice.JsonDict)
    {
        return context switch
        {
            ChannelMapChoice.JsonDict => ChannelJsonMap.ContainsKey(channel) ? ChannelJsonMap[channel] : null,
            ChannelMapChoice.HttpDict => ChannelHttpMap.ContainsKey(channel) ? ChannelHttpMap[channel] : null,
            ChannelMapChoice.ChannelDict => ChannelMap.ContainsKey(channel) ? ChannelMap[channel] : null,
            _ => null
        };
    }
    
    public static Channel? FromDictKey(string jsonKey, ChannelMapChoice context = ChannelMapChoice.JsonDict)
    {
        var map = context switch
        {
            ChannelMapChoice.JsonDict => ChannelJsonMap,
            ChannelMapChoice.HttpDict => ChannelHttpMap,
            ChannelMapChoice.ChannelDict => ChannelMap,
            _ => null
        };

        if (map != null)
        {
            foreach (var pair in map)
            {
                if (pair.Value.ToLower() == jsonKey.ToLower())
                {
                    return pair.Key;
                }
            }
        }

        return null;
    }
}