using SteelSeriesAPI.Sonar.Enums;
using SteelSeriesAPI.Sonar.Models;

namespace SteelSeriesAPI.Sonar.Events;

/// <summary>The volume state of one channel across all mixes, as pushed by a Sonar event.</summary>
/// <param name="Classic">The classic-mode state, or null if absent from the event.</param>
/// <param name="Personal">The streamer-mode personal mix state, or null if streamer mode is off.</param>
/// <param name="Stream">The streamer-mode stream mix state, or null if streamer mode is off.</param>
public sealed record ChannelVolumes(VolumeSetting? Classic, VolumeSetting? Personal, VolumeSetting? Stream);

/// <summary>A full mixer state snapshot, pushed by Sonar on connection and after major changes.</summary>
/// <param name="Channels">The state of every channel present in the event, including <see cref="Channel.Master"/>.</param>
public sealed record VolumeSnapshot(IReadOnlyDictionary<Channel, ChannelVolumes> Channels);

/// <summary>An event broadcast by Sonar that this library does not yet map to a typed event.</summary>
/// <param name="EventName">The raw event name (for example "SONAR_EVENT_FEATURE_UPDATED").</param>
/// <param name="RawJson">The complete raw JSON message, for manual inspection.</param>
public sealed record SonarUnknownEvent(string EventName, string RawJson);