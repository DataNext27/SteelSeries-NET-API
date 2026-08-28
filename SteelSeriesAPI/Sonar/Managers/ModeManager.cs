using System.Text.Json;
using SteelSeriesAPI.Core;
using SteelSeriesAPI.Sonar.Enums;

namespace SteelSeriesAPI.Sonar.Managers;

/// <inheritdoc />
internal sealed class ModeManager : IModeManager
{
    private readonly ISonarTransport _transport;

    internal ModeManager(ISonarTransport transport) => _transport = transport;

    /// <inheritdoc />
    public async Task<Mode> GetAsync(CancellationToken ct = default)
    {
        using var doc = await _transport.GetAsync(SonarRoutes.GetMode, ct);

        string? raw = doc.RootElement.ValueKind == JsonValueKind.String
            ? doc.RootElement.GetString()
            : null;

        return ModeExtensions.FromApiValue(raw ?? "")
               ?? throw new SonarResponseException($"Unknown mode value '{raw}' returned by Sonar.");
    }

    /// <inheritdoc />
    public async Task SetAsync(Mode mode, CancellationToken ct = default)
    {
        await _transport.PutAsync(SonarRoutes.SetMode(mode), ct);

        // Mode switching takes ~400-600ms in practice (measured 2026-08-07).
        // Poll every 100ms with a generous 5s budget: succeeds as soon as confirmed.
        for (int attempt = 0; attempt < 50; attempt++)
        {
            if (await GetAsync(ct) == mode) return;
            await Task.Delay(100, ct);
        }

        throw new SonarResponseException(
            $"Sonar did not confirm the switch to mode '{mode}' within 5 seconds.");
    }
}