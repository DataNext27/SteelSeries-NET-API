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

    /// <summary>
    /// Volume/mute state in streamer mode. WARNING: only the "stream" sections are live;
    /// the "classic" sections return stale data (and vice versa for the classic route).
    /// Poll the route matching the current mode. Observed 2026-08-08.
    /// </summary>
    internal const string StreamerVolumes = "volumeSettings/streamer/";
    
    /// <summary>Current chat mix state (balance and availability).</summary>
    /// <remarks>Moved to the /v1/ prefix by a 2026 GG update; the unprefixed route now returns 404.</remarks>
    internal const string GetChatMix = "v1/chatMix";
    
    /// <summary>Classic-mode redirections: which device each channel is routed to.</summary>
    /// <remarks>Uses the short channel ids ("chat", "mic") - see ToClassicRedirectionKey.</remarks>
    internal const string ClassicRedirections = "classicRedirections";

    /// <summary>Streamer-mode redirections: devices and per-channel enablement of each mix.</summary>
    internal const string StreamRedirections = "streamRedirections";

    /// <summary>Whether stream monitoring ("hear what the audience hears") is enabled. Bare JSON boolean.</summary>
    internal const string StreamMonitoringEnabled = "streamRedirections/isStreamMonitoringEnabled";


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

    internal static string SetChatMix(double balance) =>
        $"v1/chatMix?balance={Format(balance)}";
    
    internal static string SetClassicRedirectionDevice(Channel channel, string deviceId) =>
        $"classicRedirections/{channel.ToClassicRedirectionKey()}/deviceId/{Uri.EscapeDataString(deviceId)}";

    internal static string SetStreamRedirectionDevice(Mix mix, string deviceId) =>
        $"streamRedirections/{mix.ToRouteKey()}/deviceId/{Uri.EscapeDataString(deviceId)}";

    internal static string SetMixChannelEnabled(Mix mix, Channel channel, bool enabled) =>
        $"streamRedirections/{mix.ToRouteKey()}/redirections/{channel.ToJsonKey()}/isEnabled/{Bool(enabled)}";

    internal static string SetStreamMonitoringEnabled(bool enabled) =>
        $"streamRedirections/isStreamMonitoringEnabled/{Bool(enabled)}";

    private static string Format(double value) =>
        value.ToString("0.00", CultureInfo.InvariantCulture);

    private static string Bool(bool value) => value ? "true" : "false";
}