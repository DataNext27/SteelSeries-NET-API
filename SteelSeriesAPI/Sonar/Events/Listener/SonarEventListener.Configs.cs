using SteelSeriesAPI.Sonar.Enums;
using SteelSeriesAPI.Sonar.Managers;
using SteelSeriesAPI.Sonar.Models;

namespace SteelSeriesAPI.Sonar.Events;

// Config selection change detection: invalidation/polling-triggered fetch + diff + granular events.
public sealed partial class SonarEventListener
{
    private readonly ConfigManager _configs;
    private readonly DebouncedRefresher _configsRefresher;
    private IReadOnlyDictionary<Channel, SonarConfig>? _selectedConfigsBaseline;

    /// <summary>
    /// Raised when Sonar broadcasts a config invalidation, without details.
    /// Most consumers should prefer <see cref="ConfigSelectionChanged"/>, which carries
    /// the affected channel and both configs.
    /// </summary>
    public event EventHandler? ConfigsInvalidated;

    /// <summary>Raised when the selected config of a channel changes.</summary>
    public event EventHandler<ConfigSelectionChange>? ConfigSelectionChanged;

    /// <summary>Fetches the selected configs, diffs them against the baseline, and raises granular events.</summary>
    private async Task RefreshSelectedConfigsAsync(CancellationToken ct)
    {
        var selected = await _configs.GetSelectedAsync(ct);

        if (_selectedConfigsBaseline is { } baseline)
        {
            foreach ((Channel channel, SonarConfig config) in selected)
            {
                SonarConfig? previous = baseline.GetValueOrDefault(channel);
                if (previous?.Id != config.Id)
                    RaiseSafely(() => ConfigSelectionChanged?.Invoke(this,
                        new ConfigSelectionChange(channel, previous, config)));
            }
        }

        _selectedConfigsBaseline = selected;
    }
}