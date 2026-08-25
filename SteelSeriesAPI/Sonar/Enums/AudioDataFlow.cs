namespace SteelSeriesAPI.Sonar.Enums;

/// <summary>The direction of an audio device.</summary>
public enum AudioDataFlow
{
    /// <summary>An output device (speakers, headphones...).</summary>
    Render,
    /// <summary>An input device (microphones, line-in...).</summary>
    Capture
}