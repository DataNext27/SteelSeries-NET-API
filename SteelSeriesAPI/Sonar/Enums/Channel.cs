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
    
    // Classic redirections use a third naming scheme: "chat"/"mic" instead of
// "chatRender"/"chatCapture" (observed 2026-08-14). Stream redirection statuses,
// in the same route family, use the volumeSettings vocabulary. Yes, really.
    private static readonly Dictionary<Channel, string> ClassicRedirectionKeys = new()
    {
        { Channel.Game, "game" },
        { Channel.Chat, "chat" },
        { Channel.Media, "media" },
        { Channel.Aux, "aux" },
        { Channel.Mic, "mic" }
        // No Master: the master channel is not a routable device.
    };

    /// <summary>Gets the key used for this channel in Sonar JSON responses.</summary>
    public static string ToJsonKey(this Channel channel) => JsonKeys[channel];

    /// <summary>Gets the key used for this channel in Sonar HTTP routes.</summary>
    public static string ToRouteKey(this Channel channel) => RouteKeys[channel];
    
    /// <summary>Gets the key used for this channel in classic redirection routes and payloads.</summary>
    /// <exception cref="ArgumentException">The channel has no redirection (Master).</exception>
    public static string ToClassicRedirectionKey(this Channel channel) =>
        ClassicRedirectionKeys.TryGetValue(channel, out var key)
            ? key
            : throw new ArgumentException($"Channel '{channel}' has no classic redirection.", nameof(channel));

    /// <summary>Resolves a Sonar JSON key back to a <see cref="Channel"/>, or null if unknown.</summary>
    public static Channel? FromJsonKey(string key)
    {
        foreach (var pair in JsonKeys)
            if (string.Equals(pair.Value, key, StringComparison.OrdinalIgnoreCase))
                return pair.Key;
        return null;
    }

    /// <summary>Resolves a classic redirection key back to a <see cref="Channel"/>, or null if unknown.</summary>
    public static Channel? FromClassicRedirectionKey(string key)
    {
        foreach (var pair in ClassicRedirectionKeys)
            if (string.Equals(pair.Value, key, StringComparison.OrdinalIgnoreCase))
                return pair.Key;
        return null;
    }
}