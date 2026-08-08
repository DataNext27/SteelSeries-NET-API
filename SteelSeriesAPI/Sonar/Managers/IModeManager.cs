using SteelSeriesAPI.Core;
using SteelSeriesAPI.Sonar.Enums;

namespace SteelSeriesAPI.Sonar.Managers;

/// <summary>Reads and switches the Sonar mixer mode.</summary>
public interface IModeManager
{
    /// <summary>Gets the current mixer mode.</summary>
    /// <param name="ct">A token to cancel the operation.</param>
    Task<Mode> GetAsync(CancellationToken ct = default);

    /// <summary>
    /// Switches the mixer mode and waits until Sonar reports the change as effective.
    /// </summary>
    /// <param name="mode">The mode to switch to.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <exception cref="SonarResponseException">Sonar did not confirm the switch in time.</exception>
    Task SetAsync(Mode mode, CancellationToken ct = default);
}