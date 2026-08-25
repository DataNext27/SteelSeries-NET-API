using System.Text.Json;
using SteelSeriesAPI.Core;
using SteelSeriesAPI.Sonar.Enums;
using SteelSeriesAPI.Sonar.Models;

namespace SteelSeriesAPI.Sonar.Managers;

/// <inheritdoc />
internal sealed class ConfigManager : IConfigManager
{
    private readonly ISonarTransport _transport;

    internal ConfigManager(ISonarTransport transport) => _transport = transport;

    /// <inheritdoc />
    public async Task<IReadOnlyList<SonarConfig>> GetAllAsync(CancellationToken ct = default)
    {
        using var doc = await _transport.GetAsync(SonarRoutes.Configs, ct);
        return ParseConfigList(doc.RootElement);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<SonarConfig>> GetAllAsync(Channel channel, CancellationToken ct = default)
    {
        var all = await GetAllAsync(ct);
        return all.Where(c => c.Channel == channel).ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<Channel, SonarConfig>> GetSelectedAsync(CancellationToken ct = default)
    {
        using var doc = await _transport.GetAsync(SonarRoutes.SelectedConfigs, ct);
        return ParseConfigList(doc.RootElement).ToDictionary(c => c.Channel);
    }

    /// <inheritdoc />
    public async Task<SonarConfig?> GetSelectedAsync(Channel channel, CancellationToken ct = default)
    {
        var selected = await GetSelectedAsync(ct);
        return selected.GetValueOrDefault(channel);
    }

    /// <inheritdoc />
    public Task SelectAsync(string configId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(configId))
            throw new ArgumentException("Config id must not be empty.", nameof(configId));

        return _transport.PutAsync(SonarRoutes.SelectConfig(configId), ct);
    }

    /// <summary>
    /// Parses a config array (both /configs and /configs/selected share the shape).
    /// Only headers are read; the EQ payloads (data/defaultData) are deliberately ignored.
    /// </summary>
    internal static IReadOnlyList<SonarConfig> ParseConfigList(JsonElement root)
    {
        var result = new List<SonarConfig>();
        if (root.ValueKind != JsonValueKind.Array) return result;

        foreach (var entry in root.EnumerateArray())
        {
            if (entry.ValueKind != JsonValueKind.Object) continue;

            string? id = entry.GetStringOrNull("id");
            string? device = entry.GetStringOrNull("virtualAudioDevice");
            Channel? channel = device is null ? null : ChannelExtensions.FromJsonKey(device);

            if (id is null || channel is null) continue; // unknown/new device kind: skip, don't crash

            result.Add(new SonarConfig(
                id,
                entry.GetStringOrNull("name") ?? "",
                channel.Value,
                entry.GetBoolOrFalse("isPreset"),
                entry.GetBoolOrFalse("isFavorite")));
        }

        return result;
    }
}