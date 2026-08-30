using SteelSeriesAPI.Sonar.Enums;
using SteelSeriesAPI.Sonar.Models;

namespace SteelSeriesAPI.Sonar.Managers;

/// <summary>Reads and controls Sonar audio redirections: device routing, mix toggles, and stream monitoring.</summary>
public interface IRedirectionsManager
{
    /// <summary>Gets the device assigned to each channel in classic mode.</summary>
    /// <param name="ct">A token to cancel the operation.</param>
    Task<IReadOnlyList<ClassicRedirection>> GetClassicRedirectionsAsync(CancellationToken ct = default);

    /// <summary>Routes a channel to a different device in classic mode.</summary>
    /// <param name="channel">The channel to reroute (Master is not routable).</param>
    /// <param name="deviceId">The Windows device identifier, as listed by the audio devices route.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    Task SetClassicDeviceAsync(Channel channel, string deviceId, CancellationToken ct = default);

    /// <summary>Gets the complete streamer-mode redirection state (both mixes and the mic passthrough).</summary>
    /// <param name="ct">A token to cancel the operation.</param>
    Task<StreamRedirections> GetStreamRedirectionsAsync(CancellationToken ct = default);

    /// <summary>Routes a streamer-mode mix to a different output device.</summary>
    Task SetMixDeviceAsync(Mix mix, string deviceId, CancellationToken ct = default);

    /// <summary>
    /// Routes the streamer-mode mic passthrough to a different capture device.
    /// </summary>
    /// <param name="deviceId">The id of the capture device to capture the mic from.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    Task SetMicDeviceAsync(string deviceId, CancellationToken ct = default);

    /// <summary>Enables or disables a channel on a streamer-mode mix (the per-channel mix toggles).</summary>
    Task SetMixChannelEnabledAsync(Mix mix, Channel channel, bool enabled, CancellationToken ct = default);

    /// <summary>Gets whether stream monitoring ("hear what the audience hears") is enabled.</summary>
    Task<bool> GetStreamMonitoringEnabledAsync(CancellationToken ct = default);

    /// <summary>Enables or disables stream monitoring.</summary>
    Task SetStreamMonitoringEnabledAsync(bool enabled, CancellationToken ct = default);
}