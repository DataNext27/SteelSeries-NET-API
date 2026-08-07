namespace SteelSeriesAPI.Sonar.Enums;

/// <summary>A Sonar virtual audio channel.</summary>
public enum Channel
{
    /// <summary>The master channel, controlling the overall output.</summary>
    Master,
    /// <summary>The game audio channel.</summary>
    Game,
    /// <summary>The chat audio channel (incoming voice).</summary>
    Chat,
    /// <summary>The media audio channel.</summary>
    Media,
    /// <summary>The auxiliary audio channel.</summary>
    Aux,
    /// <summary>The microphone channel (outgoing voice).</summary>
    Mic
}

/// <summary>Mapping helpers between <see cref="Channel"/> values and Sonar API identifiers.</summary>
public static class ChannelExtensions
{
    private static readonly Dictionary<Channel, string> JsonKeys = new()
    {
        { Channel.Master, "masters" },
        { Channel.Game, "game" },
        { Channel.Chat, "chatRender" },
        { Channel.Media, "media" },
        { Channel.Aux, "aux" },
        { Channel.Mic, "chatCapture" }
    };

    private static readonly Dictionary<Channel, string> RouteKeys = new()
    {
        { Channel.Master, "Master" },
        { Channel.Game, "game" },
        { Channel.Chat, "chatRender" },
        { Channel.Media, "media" },
        { Channel.Aux, "aux" },
        { Channel.Mic, "chatCapture" }
    };

    /// <summary>Gets the key used for this channel in Sonar JSON responses.</summary>
    public static string ToJsonKey(this Channel channel) => JsonKeys[channel];

    /// <summary>Gets the key used for this channel in Sonar HTTP routes.</summary>
    public static string ToRouteKey(this Channel channel) => RouteKeys[channel];

    /// <summary>Resolves a Sonar JSON key back to a <see cref="Channel"/>, or null if unknown.</summary>
    public static Channel? FromJsonKey(string key)
    {
        foreach (var pair in JsonKeys)
            if (string.Equals(pair.Value, key, StringComparison.OrdinalIgnoreCase))
                return pair.Key;
        return null;
    }
}