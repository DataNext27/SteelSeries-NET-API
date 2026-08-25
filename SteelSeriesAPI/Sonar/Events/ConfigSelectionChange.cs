using SteelSeriesAPI.Sonar.Enums;
using SteelSeriesAPI.Sonar.Models;

namespace SteelSeriesAPI.Sonar.Events;

/// <summary>The selected config of a channel changed.</summary>
/// <param name="Channel">The channel whose selection changed.</param>
/// <param name="PreviousConfig">The config that was selected before, or null if unknown.</param>
/// <param name="NewConfig">The config that is selected now.</param>
public sealed record ConfigSelectionChange(Channel Channel, SonarConfig? PreviousConfig, SonarConfig NewConfig)
{
    /// <summary>The display name of the newly selected config. Shortcut for NewConfig.Name.</summary>
    public string NewConfigName => NewConfig.Name;
}