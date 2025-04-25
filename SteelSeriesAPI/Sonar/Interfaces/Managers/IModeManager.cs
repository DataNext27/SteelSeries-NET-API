using SteelSeriesAPI.Sonar.Enums;

namespace SteelSeriesAPI.Sonar.Interfaces.Managers;

internal interface IModeManager
{
    /// <summary>
    /// Get the current mode used by Sonar
    /// </summary>
    /// <returns>A <see cref="Mode"/>, either Classic or Streamer</returns>
    Mode Get();
    
    /// <summary>
    /// Set the <see cref="Mode"/> Sonar will be using
    /// </summary>
    /// <param name="mode">The <see cref="Mode"/> you want to set</param>
    void Set(Mode mode);
}