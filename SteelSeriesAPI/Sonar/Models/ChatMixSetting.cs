namespace SteelSeriesAPI.Sonar.Models;

/// <summary>The chat mix state of the Sonar mixer.</summary>
/// <param name="Balance">
/// The game/chat balance, from -1.0 (game only) to +1.0 (chat only). 0.0 is neutral.
/// </param>
/// <param name="State">
/// The availability state as reported by Sonar (for example whether a compatible
/// device is selected). Raw API value, not yet mapped to an enum.
/// </param>
public record ChatMixSetting(double Balance, string? State);