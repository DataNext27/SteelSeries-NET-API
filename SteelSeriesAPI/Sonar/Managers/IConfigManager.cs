using SteelSeriesAPI.Sonar.Enums;
using SteelSeriesAPI.Sonar.Models;

namespace SteelSeriesAPI.Sonar.Managers;

/// <summary>Lists and selects Sonar audio configs (presets).</summary>
public interface IConfigManager
{
    /// <summary>Gets all configs, user-created and presets, across all channels.</summary>
    /// <param name="ct">A token to cancel the operation.</param>
    Task<IReadOnlyList<SonarConfig>> GetAllAsync(CancellationToken ct = default);

    /// <summary>Gets all configs applicable to one channel.</summary>
    /// <param name="channel">The channel to list configs for.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    Task<IReadOnlyList<SonarConfig>> GetAllAsync(Channel channel, CancellationToken ct = default);

    /// <summary>Gets the currently selected config of each channel.</summary>
    /// <param name="ct">A token to cancel the operation.</param>
    Task<IReadOnlyDictionary<Channel, SonarConfig>> GetSelectedAsync(CancellationToken ct = default);

    /// <summary>Gets the currently selected config of one channel, or null if none is reported.</summary>
    /// <param name="channel">The channel to query.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    Task<SonarConfig?> GetSelectedAsync(Channel channel, CancellationToken ct = default);

    /// <summary>Selects a config. The affected channel is determined by the config itself.</summary>
    /// <param name="configId">The id of the config to select, as returned by the listing methods.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    Task SelectAsync(string configId, CancellationToken ct = default);
}