using SteelSeriesAPI.Sonar.Enums;

namespace SteelSeriesAPI.Sonar.Events;

/// <summary>A detected change of the Sonar mixer mode.</summary>
/// <param name="PreviousMode">The mode before the change.</param>
/// <param name="NewMode">The mode after the change.</param>
public sealed record ModeChange(Mode PreviousMode, Mode NewMode);