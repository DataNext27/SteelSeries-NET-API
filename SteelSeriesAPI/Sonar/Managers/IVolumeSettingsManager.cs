using SteelSeriesAPI.Sonar.Enums;
using SteelSeriesAPI.Sonar.Models;

namespace SteelSeriesAPI.Sonar.Managers;

/// <summary>Reads and controls the volume and mute state of Sonar channels.</summary>
public interface IVolumeSettingsManager
{
    /// <summary>Gets the volume and mute state of a channel in classic mode.</summary>
    /// <param name="channel">The channel to read.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    Task<VolumeSetting> GetAsync(Channel channel, CancellationToken ct = default);

    /// <summary>Gets the volume and mute state of a channel for a specific streamer-mode mix.</summary>
    Task<VolumeSetting> GetAsync(Channel channel, Mix mix, CancellationToken ct = default);

    /// <summary>Sets the volume of a channel in classic mode.</summary>
    /// <param name="channel">The channel to modify.</param>
    /// <param name="volume">The volume level, from 0.0 to 1.0.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <exception cref="ArgumentOutOfRangeException">Volume is outside the 0.0–1.0 range.</exception>
    Task SetVolumeAsync(Channel channel, double volume, CancellationToken ct = default);

    /// <summary>Sets the volume of a channel for a specific streamer-mode mix.</summary>
    Task SetVolumeAsync(Channel channel, Mix mix, double volume, CancellationToken ct = default);

    /// <summary>Mutes or unmutes a channel in classic mode.</summary>
    Task SetMuteAsync(Channel channel, bool muted, CancellationToken ct = default);

    /// <summary>Mutes or unmutes a channel for a specific streamer-mode mix.</summary>
    Task SetMuteAsync(Channel channel, Mix mix, bool muted, CancellationToken ct = default);
}