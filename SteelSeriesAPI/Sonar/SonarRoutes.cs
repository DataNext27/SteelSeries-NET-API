using System.Globalization;
using SteelSeriesAPI.Sonar.Enums;

namespace SteelSeriesAPI.Sonar;

/// <summary>
/// Central registry of every Sonar HTTP route used by this library.
/// If a SteelSeries GG update changes a route, this is the only file to update.
/// </summary>
internal static class SonarRoutes
{
    /// <summary>Current mixer mode, returned as a bare JSON string ("classic" or "stream").</summary>
    internal const string GetMode = "mode/";
    
    /// <summary>Volume/mute state of all channels in classic mode.</summary>
    internal const string ClassicVolumes = "volumeSettings/classic/";

    /// <summary>Volume/mute state of all channels/mixes in streamer mode.</summary>
    internal const string StreamerVolumes = "volumeSettings/streamer/";

    // Note: the Sonar API is inconsistent by design ("Volume"/"Mute" capitalized
    // in classic routes, "volume"/"isMuted" lowercase in streamer routes).
    // Verified against GG on 2026-08-04.

    internal static string SetMode(Mode mode) => $"mode/{mode.ToApiValue()}";
    
    internal static string SetClassicVolume(Channel channel, double volume) =>
        $"volumeSettings/classic/{channel.ToRouteKey()}/Volume/{Format(volume)}";

    internal static string SetClassicMute(Channel channel, bool muted) =>
        $"volumeSettings/classic/{channel.ToRouteKey()}/Mute/{Bool(muted)}";

    internal static string SetStreamerVolume(Mix mix, Channel channel, double volume) =>
        $"volumeSettings/streamer/{mix.ToRouteKey()}/{channel.ToRouteKey()}/volume/{Format(volume)}";

    internal static string SetStreamerMute(Mix mix, Channel channel, bool muted) =>
        $"volumeSettings/streamer/{mix.ToRouteKey()}/{channel.ToRouteKey()}/isMuted/{Bool(muted)}";

    private static string Format(double value) =>
        value.ToString("0.00", CultureInfo.InvariantCulture);

    private static string Bool(bool value) => value ? "true" : "false";
}