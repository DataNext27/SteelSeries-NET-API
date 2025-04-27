using SteelSeriesAPI.Sonar.Enums;

namespace SteelSeriesAPI.Sonar.Interfaces.Managers;

/// <summary>
/// Manage the Sonar <see cref="Mode"/>
/// </summary>
public interface IModeManager
{
    /// <summary>
    /// Get the current <see cref="Mode"/> used by Sonar
    /// </summary>
    /// <returns><see cref="Mode"/></returns>
    Mode Get();
    
    /// <summary>
    /// Set the <see cref="Mode"/> Sonar will be using
    /// </summary>
    /// <param name="mode"><see cref="Mode"/></param>
    void Set(Mode mode);
}