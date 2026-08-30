using Microsoft.Extensions.Logging;
using SteelSeriesAPI.Sonar.Enums;
using SteelSeriesAPI.Sonar.Managers;
using SteelSeriesAPI.Sonar.Models;

namespace SteelSeriesAPI.Sonar.Events;

// Redirection change detection: invalidation/polling-triggered fetch + diff + granular events.
public sealed partial class SonarEventListener
{
    private readonly RedirectionsManager _redirections;
    private readonly DebouncedRefresher _redirectionsRefresher;
    private RedirectionsSnapshot? _redirectionsBaseline;

    /// <summary>
    /// Raised when Sonar broadcasts a redirection invalidation, without details.
    /// Most consumers should prefer the granular events: <see cref="ClassicDeviceChanged"/>,
    /// <see cref="MixDeviceChanged"/>, <see cref="MixChannelToggled"/>, <see cref="StreamMonitoringChanged"/>
    /// and <see cref="MicDeviceChanged"/>.
    /// </summary>
    public event EventHandler? RedirectionsInvalidated;

    /// <summary>Raised when a channel is routed to a different device in classic mode.</summary>
    public event EventHandler<ClassicDeviceChange>? ClassicDeviceChanged;

    /// <summary>Raised when a streamer-mode mix is routed to a different output device.</summary>
    public event EventHandler<MixDeviceChange>? MixDeviceChanged;

    /// <summary>Raised when a channel is enabled or disabled on a streamer-mode mix.</summary>
    public event EventHandler<MixChannelToggle>? MixChannelToggled;

    /// <summary>Raised when stream monitoring ("hear what the audience hears") is toggled.</summary>
    public event EventHandler<StreamMonitoringChange>? StreamMonitoringChanged;

    /// <summary>Raised when the streamer-mode mic passthrough is captured from a different device.</summary>
    public event EventHandler<MicDeviceChange>? MicDeviceChanged;

    /// <summary>The full redirection state used as a diffing baseline.</summary>
    internal sealed record RedirectionsSnapshot(
        IReadOnlyList<ClassicRedirection> Classic,
        StreamRedirections Stream,
        bool MonitoringEnabled);

    /// <summary>Fetches the full redirection state, diffs it against the baseline, and raises granular events.</summary>
    private async Task RefreshRedirectionsAsync(CancellationToken ct)
    {
        var snapshot = new RedirectionsSnapshot(
            await _redirections.GetClassicRedirectionsAsync(ct).ConfigureAwait(false),
            await _redirections.GetStreamRedirectionsAsync(ct).ConfigureAwait(false),
            await _redirections.GetStreamMonitoringEnabledAsync(ct).ConfigureAwait(false));

        if (_redirectionsBaseline is { } baseline)
        {
            RedirectionDiff diff = DiffRedirections(baseline, snapshot);

            if (!diff.IsEmpty)
            {
                _logger.LogDebug(
                    "Redirection changes detected: {Classic} classic, {MixDev} mix devices, {Toggles} toggles, monitoring changed: {Mon}, mic changed: {Mic}",
                    diff.ClassicDeviceChanges.Count, diff.MixDeviceChanges.Count,
                    diff.MixChannelToggles.Count, diff.MonitoringChange is not null, diff.MicDeviceChange is not null);
            }

            foreach (var change in diff.ClassicDeviceChanges)
                RaiseSafely(() => ClassicDeviceChanged?.Invoke(this, change));
            foreach (var change in diff.MixDeviceChanges)
                RaiseSafely(() => MixDeviceChanged?.Invoke(this, change));
            foreach (var change in diff.MixChannelToggles)
                RaiseSafely(() => MixChannelToggled?.Invoke(this, change));
            if (diff.MonitoringChange is { } monitoring)
                RaiseSafely(() => StreamMonitoringChanged?.Invoke(this, monitoring));
            if (diff.MicDeviceChange is { } micChange)
                RaiseSafely(() => MicDeviceChanged?.Invoke(this, micChange));
        }
        else
        {
            _logger.LogDebug("Redirection baseline seeded");
        }

        _redirectionsBaseline = snapshot;
    }

    /// <summary>Computes what changed between two redirection snapshots.</summary>
    internal static RedirectionDiff DiffRedirections(RedirectionsSnapshot previous, RedirectionsSnapshot current)
    {
        var classicChanges = new List<ClassicDeviceChange>();
        foreach (var cur in current.Classic)
        {
            var prev = previous.Classic.FirstOrDefault(r => r.Channel == cur.Channel);
            if (prev is not null && prev.DeviceId != cur.DeviceId)
                classicChanges.Add(new ClassicDeviceChange(cur.Channel, prev.DeviceId, cur.DeviceId));
        }

        var mixDeviceChanges = new List<MixDeviceChange>();
        var mixToggles = new List<MixChannelToggle>();
        DiffMix(previous.Stream.Personal, current.Stream.Personal);
        DiffMix(previous.Stream.Stream, current.Stream.Stream);

        void DiffMix(MixRedirection? prev, MixRedirection? cur)
        {
            if (prev is null || cur is null) return;

            if (prev.DeviceId != cur.DeviceId)
                mixDeviceChanges.Add(new MixDeviceChange(cur.Mix, prev.DeviceId, cur.DeviceId));

            foreach ((Channel channel, bool enabled) in cur.EnabledChannels)
            {
                if (prev.EnabledChannels.TryGetValue(channel, out bool wasEnabled) && wasEnabled != enabled)
                    mixToggles.Add(new MixChannelToggle(cur.Mix, channel, enabled));
            }
        }

        StreamMonitoringChange? monitoring = previous.MonitoringEnabled != current.MonitoringEnabled
            ? new StreamMonitoringChange(current.MonitoringEnabled)
            : null;

        MicDeviceChange? micChange =
            previous.Stream.Mic is { } prevMic && current.Stream.Mic is { } curMic && prevMic.DeviceId != curMic.DeviceId
                ? new MicDeviceChange(prevMic.DeviceId, curMic.DeviceId)
                : null;

        return new RedirectionDiff(classicChanges, mixDeviceChanges, mixToggles, monitoring, micChange);
    }
}