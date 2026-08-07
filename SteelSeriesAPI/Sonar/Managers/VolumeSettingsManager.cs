using System.Text.Json;
using SteelSeriesAPI.Core;
using SteelSeriesAPI.Sonar.Enums;
using SteelSeriesAPI.Sonar.Models;

namespace SteelSeriesAPI.Sonar.Managers;

/// <inheritdoc />
internal sealed class VolumeSettingsManager : IVolumeSettingsManager
{
    private readonly SonarHttpClient _client;

    internal VolumeSettingsManager(SonarHttpClient client) => _client = client;

    /// <inheritdoc />
    public async Task<VolumeSetting> GetAsync(Channel channel, CancellationToken ct = default)
    {
        using var doc = await _client.GetAsync(SonarRoutes.ClassicVolumes, ct);

        // Master lives under "masters", other channels under "devices/{key}".
        JsonElement node = channel == Channel.Master
            ? doc.RootElement.Dig("masters", "classic")
            : doc.RootElement.Dig("devices", channel.ToJsonKey(), "classic");

        return ParseSetting(node);
    }

    /// <inheritdoc />
    public async Task<VolumeSetting> GetAsync(Channel channel, Mix mix, CancellationToken ct = default)
    {
        using var doc = await _client.GetAsync(SonarRoutes.StreamerVolumes, ct);

        JsonElement node = channel == Channel.Master
            ? doc.RootElement.Dig("masters", "stream", mix.ToJsonKey())
            : doc.RootElement.Dig("devices", channel.ToJsonKey(), "stream", mix.ToJsonKey());

        return ParseSetting(node);
    }

    /// <inheritdoc />
    public Task SetVolumeAsync(Channel channel, double volume, CancellationToken ct = default)
    {
        ValidateVolume(volume);
        return _client.PutAsync(SonarRoutes.SetClassicVolume(channel, volume), ct);
    }

    /// <inheritdoc />
    public Task SetVolumeAsync(Channel channel, Mix mix, double volume, CancellationToken ct = default)
    {
        ValidateVolume(volume);
        return _client.PutAsync(SonarRoutes.SetStreamerVolume(mix, channel, volume), ct);
    }

    /// <inheritdoc />
    public Task SetMuteAsync(Channel channel, bool muted, CancellationToken ct = default) =>
        _client.PutAsync(SonarRoutes.SetClassicMute(channel, muted), ct);

    /// <inheritdoc />
    public Task SetMuteAsync(Channel channel, Mix mix, bool muted, CancellationToken ct = default) =>
        _client.PutAsync(SonarRoutes.SetStreamerMute(mix, channel, muted), ct);

    private static VolumeSetting ParseSetting(JsonElement node)
    {
        double volume = node.TryGetProperty("volume", out var v) &&
                        v.ValueKind == JsonValueKind.Number
            ? v.GetDouble() : 0.0;

        bool muted = node.TryGetProperty("muted", out var m) &&
                     m.ValueKind == JsonValueKind.True;

        return new VolumeSetting(volume, muted);
    }

    private static void ValidateVolume(double volume)
    {
        if (volume is < 0.0 or > 1.0)
            throw new ArgumentOutOfRangeException(nameof(volume), volume,
                "Volume must be between 0.0 and 1.0.");
    }
}