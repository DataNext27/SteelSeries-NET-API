using SteelSeriesAPI.Sonar.Enums;
using SteelSeriesAPI.Sonar.Models;

namespace SteelSeriesAPI.Sonar.Events;

/// <summary>A channel was routed to a different device in classic mode.</summary>
/// <param name="Channel">The channel that was rerouted.</param>
/// <param name="PreviousDeviceId">The device the channel was routed to before.</param>
/// <param name="NewDeviceId">The device the channel is routed to now.</param>
public sealed record ClassicDeviceChange(Channel Channel, string PreviousDeviceId, string NewDeviceId);

/// <summary>A streamer-mode mix was routed to a different output device.</summary>
/// <param name="Mix">The mix that was rerouted.</param>
/// <param name="PreviousDeviceId">The device the mix was routed to before.</param>
/// <param name="NewDeviceId">The device the mix is routed to now.</param>
public sealed record MixDeviceChange(Mix Mix, string PreviousDeviceId, string NewDeviceId);

/// <summary>A channel was enabled or disabled on a streamer-mode mix.</summary>
/// <param name="Mix">The affected mix.</param>
/// <param name="Channel">The toggled channel.</param>
/// <param name="IsEnabled">Whether the channel is enabled on the mix now.</param>
public sealed record MixChannelToggle(Mix Mix, Channel Channel, bool IsEnabled);

/// <summary>Stream monitoring ("hear what the audience hears") was toggled.</summary>
/// <param name="IsEnabled">Whether stream monitoring is enabled now.</param>
public sealed record StreamMonitoringChange(bool IsEnabled);

/// <summary>The streamer-mode mic passthrough was routed to a different capture device.</summary>
/// <param name="PreviousDeviceId">The device the mic was captured from before.</param>
/// <param name="NewDeviceId">The device the mic is captured from now.</param>
public sealed record MicDeviceChange(string PreviousDeviceId, string NewDeviceId);

/// <summary>Everything that changed between two redirection snapshots.</summary>
public sealed record RedirectionDiff(
    IReadOnlyList<ClassicDeviceChange> ClassicDeviceChanges,
    IReadOnlyList<MixDeviceChange> MixDeviceChanges,
    IReadOnlyList<MixChannelToggle> MixChannelToggles,
    StreamMonitoringChange? MonitoringChange,
    MicDeviceChange? MicDeviceChange)
{
    /// <summary>True when nothing actually changed between the two snapshots.</summary>
    public bool IsEmpty =>
        ClassicDeviceChanges.Count == 0 && MixDeviceChanges.Count == 0 &&
        MixChannelToggles.Count == 0 && MonitoringChange is null && MicDeviceChange is null;
}