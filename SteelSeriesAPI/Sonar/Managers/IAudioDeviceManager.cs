using SteelSeriesAPI.Sonar.Enums;
using SteelSeriesAPI.Sonar.Models;

namespace SteelSeriesAPI.Sonar.Managers;

/// <summary>Lists the audio devices known to Sonar.</summary>
public interface IAudioDeviceManager
{
    /// <summary>Gets all devices, physical and Sonar virtual.</summary>
    /// <param name="ct">A token to cancel the operation.</param>
    Task<IReadOnlyList<AudioDevice>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets the devices of one direction, excluding Sonar virtual devices by default.
    /// This is the list to offer when picking a redirection target.
    /// </summary>
    /// <param name="dataFlow">The device direction to list.</param>
    /// <param name="includeSonarVirtual">Whether to include Sonar's own virtual devices.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    Task<IReadOnlyList<AudioDevice>> GetAllAsync(
        AudioDataFlow dataFlow, bool includeSonarVirtual = false, CancellationToken ct = default);
}