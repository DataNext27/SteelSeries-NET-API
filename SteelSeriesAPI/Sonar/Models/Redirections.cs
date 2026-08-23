using SteelSeriesAPI.Sonar.Enums;

namespace SteelSeriesAPI.Sonar.Models;

/// <summary>The output (or input, for Mic) device assigned to a channel in classic mode.</summary>
/// <param name="Channel">The channel this redirection belongs to.</param>
/// <param name="DeviceId">The Windows device identifier the channel is routed to.</param>
/// <param name="IsRunning">Whether the redirection is currently active.</param>
public sealed record ClassicRedirection(Channel Channel, string DeviceId, bool IsRunning);

/// <summary>The state of one streamer-mode mix: its output device and which channels feed it.</summary>
/// <param name="Mix">The mix this redirection belongs to.</param>
/// <param name="DeviceId">The Windows device identifier the mix is routed to.</param>
/// <param name="IsRunning">Whether the mix redirection is currently active.</param>
/// <param name="EnabledChannels">Which channels are enabled (toggled on) for this mix.</param>
public sealed record MixRedirection(
    Mix Mix,
    string DeviceId,
    bool IsRunning,
    IReadOnlyDictionary<Channel, bool> EnabledChannels);

/// <summary>The microphone passthrough state in streamer mode.</summary>
/// <param name="DeviceId">The Windows device identifier of the physical microphone.</param>
/// <param name="IsRunning">Whether the passthrough is currently active.</param>
public sealed record MicRedirection(string DeviceId, bool IsRunning);

/// <summary>The complete streamer-mode redirection state.</summary>
/// <param name="Personal">The personal (monitoring) mix, or null if absent from the response.</param>
/// <param name="Stream">The stream (streaming) mix, or null if absent from the response.</param>
/// <param name="Mic">The microphone passthrough, or null if absent from the response.</param>
public sealed record StreamRedirections(MixRedirection? Personal, MixRedirection? Stream, MicRedirection? Mic);