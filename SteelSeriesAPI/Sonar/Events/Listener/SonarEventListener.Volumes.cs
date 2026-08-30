using System.Text.Json;
using Microsoft.Extensions.Logging;
using SteelSeriesAPI.Core;
using SteelSeriesAPI.Sonar.Enums;
using SteelSeriesAPI.Sonar.Managers;
using SteelSeriesAPI.Sonar.Models;

namespace SteelSeriesAPI.Sonar.Events;

// Volume and mode detection: the polling loop, the snapshot parsers, and the mode-aware diff.
public sealed partial class SonarEventListener
{
    /// <summary>
    /// Raised when Sonar pushes a full volume snapshot (on connection, after major changes
    /// such as a mode switch, and on OS/hardware-initiated volume changes).
    /// Most consumers should prefer <see cref="VolumeChanged"/>, which carries granular diffs.
    /// </summary>
    public event EventHandler<VolumeSnapshot>? VolumeDataReceived;

    /// <summary>Raised when polling detects a volume or mute change. Requires <see cref="PollingInterval"/>.</summary>
    public event EventHandler<VolumeChange>? VolumeChanged;

    /// <summary>Raised when polling detects a mixer mode change. Requires <see cref="PollingInterval"/>.</summary>
    public event EventHandler<ModeChange>? ModeChanged;

    /// <summary>
    /// Polls the mode and the matching volume route, raising granular events on differences.
    /// Each volumeSettings route only reliably reflects its own mode's values (observed
    /// 2026-08-08: the other mode's section returns stale data), hence the mode-aware routing.
    /// Also runs the redirection and config refreshes on the same cadence, because Sonar
    /// does not broadcast changes received through its own HTTP API.
    /// </summary>
    private async Task RunPollingAsync(TimeSpan interval, CancellationToken ct)
    {
        var modeManager = new ModeManager(_httpClient);
        VolumeSnapshot? baseline = null;
        Mode? baselineMode = null;

        while (!ct.IsCancellationRequested)
        {
            try { await Task.Delay(interval, ct).ConfigureAwait(false); }
            catch (OperationCanceledException) { break; }

            try
            {
                Mode mode = await modeManager.GetAsync(ct).ConfigureAwait(false);

                string route = mode == Mode.Streamer
                    ? SonarRoutes.StreamerVolumes
                    : SonarRoutes.ClassicVolumes;

                using var doc = await _httpClient.GetAsync(route, ct).ConfigureAwait(false);
                var snapshot = ParseVolumeSnapshot(doc.RootElement);

                if (baselineMode is not null && baselineMode != mode)
                {
                    Mode previous = baselineMode.Value;
                    RaiseSafely(() => ModeChanged?.Invoke(this, new ModeChange(previous, mode)));
                }

                // Only diff against a baseline captured in the same mode: comparing across
                // modes would produce spurious events from the stale sections.
                if (baseline is not null && baselineMode == mode)
                {
                    foreach (VolumeChange change in Diff(baseline, snapshot, mode))
                        RaiseSafely(() => VolumeChanged?.Invoke(this, change));
                }

                baseline = snapshot;
                baselineMode = mode;

                await _redirectionsRefresher.RunNowAsync(ct).ConfigureAwait(false);
                await _configsRefresher.RunNowAsync(ct).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested) { break; }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Polling tick failed");
            }
        }
    }

    /// <summary>
    /// Computes the volume changes between two snapshots, comparing only the values
    /// that are reliable in the given mode.
    /// </summary>
    internal static IEnumerable<VolumeChange> Diff(VolumeSnapshot previous, VolumeSnapshot current, Mode mode)
    {
        foreach ((Channel channel, ChannelVolumes cur) in current.Channels)
        {
            if (!previous.Channels.TryGetValue(channel, out var prev)) continue;

            if (mode == Mode.Classic)
            {
                if (Changed(prev.Classic, cur.Classic))
                    yield return new VolumeChange(channel, null, prev.Classic!, cur.Classic!);
            }
            else
            {
                if (Changed(prev.Personal, cur.Personal))
                    yield return new VolumeChange(channel, Mix.Personal, prev.Personal!, cur.Personal!);
                if (Changed(prev.Stream, cur.Stream))
                    yield return new VolumeChange(channel, Mix.Stream, prev.Stream!, cur.Stream!);
            }
        }

        static bool Changed(VolumeSetting? previous, VolumeSetting? current) =>
            previous is not null && current is not null && previous != current;
    }

    /// <summary>Parses a SONAR_EVENT_VOLUME_DATA payload. Same shape as GET volumeSettings/streamer/.</summary>
    internal static VolumeSnapshot ParseVolumeSnapshot(JsonElement data)
    {
        var channels = new Dictionary<Channel, ChannelVolumes>();

        if (data.ValueKind == JsonValueKind.Object &&
            data.TryGetProperty("masters", out var masters))
        {
            channels[Channel.Master] = ParseChannelVolumes(masters);
        }

        if (data.ValueKind == JsonValueKind.Object &&
            data.TryGetProperty("devices", out var devices) &&
            devices.ValueKind == JsonValueKind.Object)
        {
            foreach (var device in devices.EnumerateObject())
            {
                Channel? channel = ChannelExtensions.FromJsonKey(device.Name);
                if (channel is null) continue; // unknown channel added by a future GG update: skip, don't crash

                channels[channel.Value] = ParseChannelVolumes(device.Value);
            }
        }

        return new VolumeSnapshot(channels);
    }

    private static ChannelVolumes ParseChannelVolumes(JsonElement node)
    {
        VolumeSetting? classic = null, personal = null, stream = null;

        if (node.ValueKind == JsonValueKind.Object)
        {
            if (node.TryGetProperty("classic", out var c) && c.ValueKind == JsonValueKind.Object)
                classic = ParseSetting(c);

            if (node.TryGetProperty("stream", out var st) && st.ValueKind == JsonValueKind.Object)
            {
                if (st.TryGetProperty(Mix.Personal.ToJsonKey(), out var p) && p.ValueKind == JsonValueKind.Object)
                    personal = ParseSetting(p);
                if (st.TryGetProperty(Mix.Stream.ToJsonKey(), out var sm) && sm.ValueKind == JsonValueKind.Object)
                    stream = ParseSetting(sm);
            }
        }

        return new ChannelVolumes(classic, personal, stream);
    }

    private static VolumeSetting ParseSetting(JsonElement node)
    {
        double volume = node.TryGetProperty("volume", out var v) &&
                        v.ValueKind == JsonValueKind.Number ? v.GetDouble() : 0.0;
        bool muted = node.GetBoolOrFalse("muted");
        return new VolumeSetting(volume, muted);
    }
}