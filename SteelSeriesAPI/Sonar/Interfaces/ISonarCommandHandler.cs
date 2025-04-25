using SteelSeriesAPI.Sonar.Enums;

namespace SteelSeriesAPI.Sonar.Interfaces;

public interface ISonarCommandHandler
{
    /// <summary>
    /// Set the <see cref="Mode"/> Sonar will be using
    /// </summary>
    /// <param name="mode">The <see cref="Mode"/> you want to set</param>
    void SetMode(Mode mode);

    /// <summary>
    /// Enable or disable the Redirection of the chosen Sonar <see cref="Mix"/> of the chosen Sonar <see cref="Channel"/>
    /// </summary>
    /// <param name="newState">The new state of the Redirection</param>
    /// <param name="channel">The Sonar <see cref="Channel"/> you want to un/mute a Sonar redirection mix</param>
    /// <param name="mix">The Sonar <see cref="Mix"/> you want to un/mute</param>
    void SetRedirectionState(bool newState, Channel channel, Mix mix);

    /// <summary>
    /// Listen to what your audience hear
    /// </summary>
    /// <param name="newState">The new state, un/muted</param>
    void SetAudienceMonitoringState(bool newState);

    /// <summary>
    /// Redirect the audio of an app to a Sonar <see cref="Channel"/>
    /// </summary>
    /// <param name="pId">The process ID of the app</param>
    /// <param name="channel">The Sonar <see cref="Channel"/> you want to set the app audio</param>
    void SetProcessToDeviceRouting(int pId, Channel channel);
}