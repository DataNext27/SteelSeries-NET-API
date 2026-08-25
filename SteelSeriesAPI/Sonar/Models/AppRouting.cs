using SteelSeriesAPI.Sonar.Enums;

namespace SteelSeriesAPI.Sonar.Models;

/// <summary>An application audio session known to Sonar.</summary>
/// <param name="ProcessName">The process name (e.g. "brave").</param>
/// <param name="ProcessId">The Windows process id. WARNING: changes on every launch - never persist it.</param>
/// <param name="DisplayName">The human-readable name (e.g. "Brave").</param>
/// <param name="IsSystemSound">True for the Windows system-sounds session.</param>
/// <param name="State">The raw session state as reported by Sonar ("active", "inactive"...).</param>
public sealed record AudioSessionInfo(
    string ProcessName,
    int ProcessId,
    string DisplayName,
    bool IsSystemSound,
    string? State)
{
    /// <summary>True when the session is currently playing/capturing audio.</summary>
    public bool IsActive => string.Equals(State, "active", StringComparison.OrdinalIgnoreCase);
}

/// <summary>The audio sessions currently attached to one device.</summary>
/// <param name="DeviceId">The Windows device identifier.</param>
/// <param name="Role">The raw Sonar role of the device ("game", "stream", "none"...).</param>
/// <param name="Channel">The Sonar channel this device carries, or null (physical devices, stream mixes).</param>
/// <param name="DataFlow">Whether this is an output or input device.</param>
/// <param name="Sessions">The sessions attached to this device, ghosts of past routings included.</param>
public sealed record DeviceRouting(
    string DeviceId,
    string Role,
    Channel? Channel,
    AudioDataFlow DataFlow,
    IReadOnlyList<AudioSessionInfo> Sessions);