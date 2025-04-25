using SteelSeriesAPI.Sonar.Enums;

namespace SteelSeriesAPI.Sonar.Interfaces.Managers;

internal interface IRedirectionStateManager
{
    /// <summary>
    /// Get the mute state of the Redirection of the chosen Sonar <see cref="Mix"/> of the chosen Sonar <see cref="Channel"/>
    /// </summary>
    /// <param name="channel">The Sonar <see cref="Channel"/> you want the mute state of a Redirection mix</param>
    /// <param name="mix">The Sonar <see cref="Mix"/> you want to get the mute state</param>
    /// <returns>The current state, un/muted</returns>
    bool Get(Channel channel, Mix mix);
    
    /// <summary>
    /// Enable or disable the Redirection of the chosen Sonar <see cref="Mix"/> of the chosen Sonar <see cref="Channel"/>
    /// </summary>
    /// <param name="newState">The new state of the Redirection</param>
    /// <param name="channel">The Sonar <see cref="Channel"/> you want to un/mute a Sonar redirection mix</param>
    /// <param name="mix">The Sonar <see cref="Mix"/> you want to un/mute</param>
    void Set(bool newState, Channel channel, Mix mix);
}