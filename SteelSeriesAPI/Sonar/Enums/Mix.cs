namespace SteelSeriesAPI.Sonar.Enums;

/// <summary>
/// A streamer-mode output mix.
/// When streamer mode is enabled, Sonar splits the audio into two
/// independent mixes that can be balanced separately.
/// </summary>
public enum Mix
{
    /// <summary>The personal mix: what the streamer hears in their own headset.</summary>
    Personal,

    /// <summary>The stream mix: what the audience hears through the streaming software.</summary>
    Stream
}

/// <summary>Mapping helpers between <see cref="Mix"/> values and Sonar API identifiers.</summary>
public static class MixExtensions
{
    // Internal API ids differ from the UI names:
    // the personal mix is "monitoring", the stream mix is "streaming".
    private static readonly Dictionary<Mix, string> ApiKeys = new()
    {
        { Mix.Personal, "monitoring" },
        { Mix.Stream, "streaming" }
    };

    /// <summary>Gets the identifier used for this mix in Sonar JSON responses.</summary>
    public static string ToJsonKey(this Mix mix) => ApiKeys[mix];

    /// <summary>Gets the identifier used for this mix in Sonar HTTP routes.</summary>
    public static string ToRouteKey(this Mix mix) => ApiKeys[mix];

    /// <summary>Resolves a Sonar API identifier back to a <see cref="Mix"/>, or null if unknown.</summary>
    public static Mix? FromJsonKey(string key)
    {
        foreach (var pair in ApiKeys)
            if (string.Equals(pair.Value, key, StringComparison.OrdinalIgnoreCase))
                return pair.Key;
        return null;
    }
}