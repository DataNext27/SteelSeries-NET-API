using System.Text.Json;
using SteelSeriesAPI.Core;
using SteelSeriesAPI.Sonar.Models;

namespace SteelSeriesAPI.Sonar.Managers;

/// <inheritdoc />
internal sealed class ChatMixManager : IChatMixManager
{
    private readonly ISonarTransport _transport;

    internal ChatMixManager(ISonarTransport transport) => _transport = transport;

    /// <inheritdoc />
    public async Task<ChatMixSetting> GetAsync(CancellationToken ct = default)
    {
        using var doc = await _transport.GetAsync(SonarRoutes.GetChatMix, ct);
        var root = doc.RootElement;

        double balance = root.TryGetProperty("balance", out var b) &&
                         b.ValueKind == JsonValueKind.Number
            ? b.GetDouble() : 0.0;

        string? state = root.TryGetProperty("state", out var s) &&
                        s.ValueKind == JsonValueKind.String
            ? s.GetString() : null;

        return new ChatMixSetting(balance, state);
    }

    /// <inheritdoc />
    public Task SetAsync(double balance, CancellationToken ct = default)
    {
        if (double.IsNaN(balance) || balance is < -1.0 or > 1.0)
            throw new ArgumentOutOfRangeException(nameof(balance), balance,
                "Chat mix balance must be between -1.0 and 1.0.");

        return _transport.PutAsync(SonarRoutes.SetChatMix(balance), ct);
    }
}