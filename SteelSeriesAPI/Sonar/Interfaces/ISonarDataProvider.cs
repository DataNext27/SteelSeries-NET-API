using SteelSeriesAPI.Sonar.Enums;
using SteelSeriesAPI.Sonar.Models;

namespace SteelSeriesAPI.Sonar.Interfaces;

public interface ISonarDataProvider
{
    /// <summary>
    /// Get the current mode used by Sonar
    /// </summary>
    /// <returns>A <see cref="Mode"/>, either Classic or Streamer</returns>
    Mode GetMode();

    /// <summary>
    /// Get the mute state of the Redirection of the chosen Sonar <see cref="Mix"/> of the chosen Sonar <see cref="Channel"/>
    /// </summary>
    /// <param name="channel">The Sonar <see cref="Channel"/> you want the mute state of a Redirection mix</param>
    /// <param name="mix">The Sonar <see cref="Mix"/> you want to get the mute state</param>
    /// <returns>The current state, un/muted</returns>
    bool GetRedirectionState(Channel channel, Mix mix);

    /// <summary>
    /// Get the current state of the Audience Monitoring
    /// </summary>
    /// <returns>The current state, un/muted</returns>
    bool GetAudienceMonitoringState();
    
    /// <summary>
    /// Get the apps which their audio is redirected to a Sonar <see cref="Channel"/>
    /// </summary>
    /// <param name="channel">The Sonar <see cref="Channel"/> you want the associated processes</param>
    /// <returns>A list of <see cref="RoutedProcess"/></returns>
    IEnumerable<RoutedProcess> GetRoutedProcess(Channel channel);
}