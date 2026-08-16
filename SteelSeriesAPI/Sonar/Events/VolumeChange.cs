using SteelSeriesAPI.Sonar.Enums;
using SteelSeriesAPI.Sonar.Models;

namespace SteelSeriesAPI.Sonar.Events;

/// <summary>
/// A detected change in the volume or mute state of one channel on one mix.
/// </summary>
/// <param name="Channel">The channel that changed (Game, Chat, Master...).</param>
/// <param name="Mix">
/// The affected streamer-mode mix (Personal or Stream), or null when the change
/// happened on the classic-mode value.
/// </param>
/// <param name="PreviousState">The full volume/mute state before the change.</param>
/// <param name="NewState">The full volume/mute state after the change.</param>
public sealed record VolumeChange(
    Channel Channel,
    Mix? Mix,
    VolumeSetting PreviousState,
    VolumeSetting NewState)
{
    /// <summary>The volume level after the change, from 0.0 to 1.0.</summary>
    public double NewVolume => NewState.Volume;

    /// <summary>The volume level before the change, from 0.0 to 1.0.</summary>
    public double PreviousVolume => PreviousState.Volume;

    /// <summary>Whether the channel is muted after the change.</summary>
    public bool IsMuted => NewState.Muted;

    /// <summary>Whether the channel was muted before the change.</summary>
    public bool WasMuted => PreviousState.Muted;

    /// <summary>True when the mute state itself changed (the user pressed mute/unmute).</summary>
    public bool MuteToggled => WasMuted != IsMuted;
}