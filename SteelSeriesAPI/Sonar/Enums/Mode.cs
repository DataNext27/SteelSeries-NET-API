namespace SteelSeriesAPI.Sonar.Enums;

/// <summary>The operating mode of the Sonar mixer.</summary>
public enum Mode
{
    /// <summary>Classic mode: a single output mix with one volume per channel.</summary>
    Classic,

    /// <summary>Streamer mode: two independent output mixes (personal and stream).</summary>
    Streamer
}

/// <summary>Mapping helpers between <see cref="Mode"/> values and Sonar API identifiers.</summary>
public static class ModeExtensions
{
    /// <summary>Gets the identifier used for this mode by the Sonar API.</summary>
    public static string ToApiValue(this Mode mode) =>
        mode == Mode.Streamer ? "stream" : "classic";

    /// <summary>Resolves a Sonar API identifier back to a <see cref="Mode"/>, or null if unknown.</summary>
    public static Mode? FromApiValue(string value) => value switch
    {
        "classic" => Mode.Classic,
        "stream" => Mode.Streamer,
        _ => null
    };
}