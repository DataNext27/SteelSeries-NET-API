using SteelSeriesAPI.Sonar.Models;

namespace SteelSeriesAPI.Sonar.Managers;

/// <summary>Reads and controls the Sonar chat mix (game/chat balance).</summary>
public interface IChatMixManager
{
    /// <summary>Gets the current chat mix balance and availability state.</summary>
    /// <param name="ct">A token to cancel the operation.</param>
    Task<ChatMixSetting> GetAsync(CancellationToken ct = default);

    /// <summary>Sets the chat mix balance.</summary>
    /// <param name="balance">The balance, from -1.0 (game only) to +1.0 (chat only).</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <exception cref="ArgumentOutOfRangeException">Balance is outside the -1.0–1.0 range.</exception>
    Task SetAsync(double balance, CancellationToken ct = default);
}