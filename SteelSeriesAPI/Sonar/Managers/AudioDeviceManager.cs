using System.Text.Json;
using SteelSeriesAPI.Core;
using SteelSeriesAPI.Sonar.Enums;
using SteelSeriesAPI.Sonar.Models;

namespace SteelSeriesAPI.Sonar.Managers;

/// <inheritdoc />
internal sealed class AudioDeviceManager : IAudioDeviceManager
{
    private readonly ISonarTransport _transport;

    internal AudioDeviceManager(ISonarTransport transport) => _transport = transport;

    /// <inheritdoc />
    public async Task<IReadOnlyList<AudioDevice>> GetAllAsync(CancellationToken ct = default)
    {
        using var doc = await _transport.GetAsync(SonarRoutes.AudioDevices, ct).ConfigureAwait(false);
        return ParseDevices(doc.RootElement);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<AudioDevice>> GetAllAsync(
        AudioDataFlow dataFlow, bool includeSonarVirtual = false, CancellationToken ct = default)
    {
        var all = await GetAllAsync(ct).ConfigureAwait(false);
        return all
            .Where(d => d.DataFlow == dataFlow && (includeSonarVirtual || !d.IsSonarVirtual))
            .ToList();
    }

    /// <summary>Parses the audioDevices array. Entries with unknown data flows are skipped.</summary>
    internal static IReadOnlyList<AudioDevice> ParseDevices(JsonElement root)
    {
        var result = new List<AudioDevice>();
        if (root.ValueKind != JsonValueKind.Array) return result;

        foreach (var entry in root.EnumerateArray())
        {
            string? id = entry.GetStringOrNull("id");
            if (id is null) continue;

            AudioDataFlow? dataFlow = entry.GetStringOrNull("dataFlow") switch
            {
                "render" => AudioDataFlow.Render,
                "capture" => AudioDataFlow.Capture,
                _ => null // unknown flow from a future GG update: skip
            };
            if (dataFlow is null) continue;

            bool isVirtual = entry.GetBoolOrFalse("isVad");
            Channel? sonarChannel = isVirtual && entry.GetStringOrNull("role") is { } role
                ? ChannelExtensions.FromJsonKey(role)
                : null;

            result.Add(new AudioDevice(
                id,
                entry.GetStringOrNull("friendlyName") ?? id,
                dataFlow.Value,
                isVirtual,
                sonarChannel));
        }

        return result;
    }
}