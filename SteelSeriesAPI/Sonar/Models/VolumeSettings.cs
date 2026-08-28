namespace SteelSeriesAPI.Sonar.Models;

/// <summary>The volume state of a single Sonar channel.</summary>
/// <param name="Volume">The volume level, from 0.0 (silent) to 1.0 (full).</param>
/// <param name="Muted">Whether the channel is currently muted.</param>
public record VolumeSetting(double Volume, bool Muted);