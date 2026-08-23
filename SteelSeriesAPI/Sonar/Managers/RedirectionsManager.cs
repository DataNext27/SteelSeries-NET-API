using System.Text.Json;
using SteelSeriesAPI.Core;
using SteelSeriesAPI.Sonar.Enums;
using SteelSeriesAPI.Sonar.Models;

namespace SteelSeriesAPI.Sonar.Managers;

/// <inheritdoc />
internal sealed class RedirectionsManager : IRedirectionsManager
{
    private readonly ISonarTransport _transport;

    internal RedirectionsManager(ISonarTransport transport) => _transport = transport;

    /// <inheritdoc />
    public async Task<IReadOnlyList<ClassicRedirection>> GetClassicRedirectionsAsync(CancellationToken ct = default)
    {
        using var doc = await _transport.GetAsync(SonarRoutes.ClassicRedirections, ct);
        return ParseClassicRedirections(doc.RootElement);
    }

    /// <inheritdoc />
    public Task SetClassicDeviceAsync(Channel channel, string deviceId, CancellationToken ct = default)
    {
        ValidateDeviceId(deviceId);
        return _transport.PutAsync(SonarRoutes.SetClassicRedirectionDevice(channel, deviceId), ct);
    }

    /// <inheritdoc />
    public async Task<StreamRedirections> GetStreamRedirectionsAsync(CancellationToken ct = default)
    {
        using var doc = await _transport.GetAsync(SonarRoutes.StreamRedirections, ct);
        return ParseStreamRedirections(doc.RootElement);
    }

    /// <inheritdoc />
    public Task SetMixDeviceAsync(Mix mix, string deviceId, CancellationToken ct = default)
    {
        ValidateDeviceId(deviceId);
        return _transport.PutAsync(SonarRoutes.SetStreamRedirectionDevice(mix, deviceId), ct);
    }

    /// <inheritdoc />
    public Task SetMixChannelEnabledAsync(Mix mix, Channel channel, bool enabled, CancellationToken ct = default)
    {
        if (channel == Channel.Master)
            throw new ArgumentException("The Master channel cannot be toggled on a mix.", nameof(channel));

        return _transport.PutAsync(SonarRoutes.SetMixChannelEnabled(mix, channel, enabled), ct);
    }

    /// <inheritdoc />
    public async Task<bool> GetStreamMonitoringEnabledAsync(CancellationToken ct = default)
    {
        using var doc = await _transport.GetAsync(SonarRoutes.StreamMonitoringEnabled, ct);
        return doc.RootElement.ValueKind == JsonValueKind.True;
    }

    /// <inheritdoc />
    public Task SetStreamMonitoringEnabledAsync(bool enabled, CancellationToken ct = default) =>
        _transport.PutAsync(SonarRoutes.SetStreamMonitoringEnabled(enabled), ct);

    private static void ValidateDeviceId(string deviceId)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
            throw new ArgumentException("Device id must not be empty.", nameof(deviceId));
    }

    /// <summary>Parses the classicRedirections array. Unknown channel ids are skipped.</summary>
    internal static IReadOnlyList<ClassicRedirection> ParseClassicRedirections(JsonElement root)
    {
        var result = new List<ClassicRedirection>();
        if (root.ValueKind != JsonValueKind.Array) return result;

        foreach (var entry in root.EnumerateArray())
        {
            if (entry.ValueKind != JsonValueKind.Object) continue;

            string? id = entry.TryGetProperty("id", out var i) && i.ValueKind == JsonValueKind.String
                ? i.GetString() : null;
            Channel? channel = id is null ? null : ChannelExtensions.FromClassicRedirectionKey(id);
            if (channel is null) continue; // unknown channel from a future GG update: skip

            result.Add(new ClassicRedirection(
                channel.Value,
                GetString(entry, "deviceId") ?? "",
                GetBool(entry, "isRunning")));
        }

        return result;
    }

    /// <summary>Parses the streamRedirections array (both mixes and the mic passthrough).</summary>
    internal static StreamRedirections ParseStreamRedirections(JsonElement root)
    {
        MixRedirection? personal = null, stream = null;
        MicRedirection? mic = null;

        if (root.ValueKind != JsonValueKind.Array)
            return new StreamRedirections(null, null, null);

        foreach (var entry in root.EnumerateArray())
        {
            if (entry.ValueKind != JsonValueKind.Object) continue;

            string? id = GetString(entry, "streamRedirectionId");
            string deviceId = GetString(entry, "deviceId") ?? "";
            bool isRunning = GetBool(entry, "isRunning");

            if (string.Equals(id, "mic", StringComparison.OrdinalIgnoreCase))
            {
                mic = new MicRedirection(deviceId, isRunning);
                continue;
            }

            Mix? mix = id is null ? null : MixExtensions.FromJsonKey(id);
            if (mix is null) continue;

            var enabled = new Dictionary<Channel, bool>();
            if (entry.TryGetProperty("status", out var status) && status.ValueKind == JsonValueKind.Array)
            {
                foreach (var role in status.EnumerateArray())
                {
                    Channel? channel = GetString(role, "role") is { } r
                        ? ChannelExtensions.FromJsonKey(r)
                        : null;
                    if (channel is null) continue;

                    enabled[channel.Value] = GetBool(role, "isEnabled");
                }
            }

            var redirection = new MixRedirection(mix.Value, deviceId, isRunning, enabled);
            if (mix == Mix.Personal) personal = redirection;
            else stream = redirection;
        }

        return new StreamRedirections(personal, stream, mic);
    }

    private static string? GetString(JsonElement obj, string name) =>
        obj.ValueKind == JsonValueKind.Object &&
        obj.TryGetProperty(name, out var p) && p.ValueKind == JsonValueKind.String
            ? p.GetString() : null;

    private static bool GetBool(JsonElement obj, string name) =>
        obj.ValueKind == JsonValueKind.Object &&
        obj.TryGetProperty(name, out var p) && p.ValueKind == JsonValueKind.True;
}