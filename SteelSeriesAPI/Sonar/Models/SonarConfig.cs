using SteelSeriesAPI.Sonar.Enums;

namespace SteelSeriesAPI.Sonar.Models;

/// <summary>
/// An audio configuration (preset) header, without its EQ/effects payload.
/// Sonar keeps one selected config per channel.
/// </summary>
/// <param name="Id">The unique identifier of the config.</param>
/// <param name="Name">The display name (e.g. "Custom", "FPS Footsteps").</param>
/// <param name="Channel">The channel this config applies to. Mic configs use the chatCapture device.</param>
/// <param name="IsPreset">True for built-in SteelSeries presets, false for user-created configs.</param>
/// <param name="IsFavorite">Whether the user marked this config as favorite.</param>
public sealed record SonarConfig(string Id, string Name, Channel Channel, bool IsPreset, bool IsFavorite);