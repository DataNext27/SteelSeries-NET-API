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

        // Mode switching is not instantaneous server-side (the V1 library used a
        // blind 100ms sleep here). Poll until Sonar confirms, with a bounded budget.
        for (int attempt = 0; attempt < 20; attempt++)
        {
            if (await GetAsync(ct) == mode) return;
            await Task.Delay(50, ct);
        }

        throw new SonarResponseException(
            $"Sonar did not confirm the switch to mode '{mode}' within 1 second.");
    }
}