namespace SteelSeriesAPI.Sonar.Enums;

public enum Channel
{
    Monitoring,
    Stream
}

public static class ChannelExtensions
{
    private static readonly Dictionary<Channel, string> ChannelMap = new Dictionary<Channel, string>
    {
        { Channel.Monitoring, "monitoring" },
        { Channel.Stream, "streaming" }
    };

    public static string ToDictKey(this Channel channel)
    {
        return ChannelMap[channel];
    }
    
    public static Channel? FromDictKey(string jsonKey)
    {
        foreach (var pair in ChannelMap)
        {
            if (pair.Value == jsonKey)
            {
                return pair.Key;
            }
        }

        return null;
    }
}