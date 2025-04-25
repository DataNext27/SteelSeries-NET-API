using SteelSeriesAPI.Sonar.Enums;
using SteelSeriesAPI.Sonar.Models;

namespace SteelSeriesAPI.Sonar.Interfaces.Managers;

internal interface IRoutedProcessManager
{
    /// <summary>
    /// Get the apps which their audio is redirected to a Sonar <see cref="Channel"/>
    /// </summary>
    /// <param name="channel">The Sonar <see cref="Channel"/> you want the associated processes</param>
    /// <returns>A list of <see cref="RoutedProcess"/></returns>
    IEnumerable<RoutedProcess> GetRoutedProcesses(Channel channel);
    
    /// <summary>
    /// Redirect the audio of an app to a Sonar <see cref="Channel"/>
    /// </summary>
    /// <param name="pId">The process ID of the app</param>
    /// <param name="channel">The Sonar <see cref="Channel"/> you want to set the app audio</param>
    void RouteProcessToChannel(int pId, Channel channel);
}