using SteelSeriesAPI.Sonar.Enums;

namespace SteelSeriesAPI.Sonar.Interfaces.Managers;

/// <summary>
/// Manage the personal and stream mix for each channel
/// </summary>
public interface IMixManager
{
    /// <summary>
    /// Get the state of the chosen Sonar <see cref="Mix"/> of the chosen Sonar <see cref="Channel"/>
    /// </summary>
    /// <param name="channel">The Sonar <see cref="Channel"/> of the <see cref="Mix"/></param>
    /// <param name="mix">The Sonar <see cref="Mix"/> you want the state of</param>
    /// <returns>The current state, activated/deactivated</returns>
    bool GetState(Channel channel, Mix mix);
    
    /// <summary>
    /// Activate or deactivate a Sonar <see cref="Mix"/> of a Sonar <see cref="Channel"/>
    /// </summary>
    /// <param name="newState">The new state of the Mix</param>
    /// <param name="channel">The Sonar <see cref="Channel"/> of the <see cref="Mix"/></param>
    /// <param name="mix">The Sonar <see cref="Mix"/> you want to activate/deactivate</param>
    void SetState(bool newState, Channel channel, Mix mix);
    
    /// <summary>
    /// Activate a Sonar <see cref="Mix"/> of a Sonar <see cref="Channel"/>
    /// </summary>
    /// <param name="channel">The Sonar <see cref="Channel"/> of the <see cref="Mix"/></param>
    /// <param name="mix">The Sonar <see cref="Mix"/> you want to activate</param>
    void Activate(Channel channel, Mix mix);
    
    /// <summary>
    /// Deactivate a Sonar <see cref="Mix"/> of a Sonar <see cref="Channel"/>
    /// </summary>
    /// <param name="channel">The Sonar <see cref="Channel"/> of the <see cref="Mix"/></param>
    /// <param name="mix">The Sonar <see cref="Mix"/> you want to deactivate</param>
    void Deactivate(Channel channel, Mix mix);
}