using SteelSeriesAPI.Sonar.Enums;
using SteelSeriesAPI.Sonar.Models;

namespace SteelSeriesAPI.Sonar.Interfaces.Managers;

/// <summary>
/// Manage routed audio processes
/// </summary>
public interface IRoutedProcessManager
{
    /// <summary>
    /// Get all audio processes that are routed to Sonar
    /// </summary>
    /// <returns>A list of <see cref="RoutedProcess"/></returns>
    IEnumerable<RoutedProcess> GetAllRoutedProcesses();
    
    /// <summary>
    /// Get all audio processes that are routed to Sonar and currently active
    /// </summary>
    /// <returns>A list of <see cref="RoutedProcess"/></returns>
    IEnumerable<RoutedProcess> GetAllActiveRoutedProcesses();
    
    /// <summary>
    /// Get all audio processes that are routed to a <see cref="Channel"/>
    /// </summary>
    /// <param name="channel"><see cref="Channel"/></param>
    /// <returns>A list of <see cref="RoutedProcess"/></returns>
    IEnumerable<RoutedProcess> GetRoutedProcesses(Channel channel);
    
    /// <summary>
    /// Get all audio processes that are routed to a <see cref="Channel"/> and currently active
    /// </summary>
    /// <param name="channel"><see cref="Channel"/></param>
    /// <returns>A list of <see cref="RoutedProcess"/></returns>
    IEnumerable<RoutedProcess> GetActiveRoutedProcesses(Channel channel);
    
    /// <summary>
    /// Get an audio process that is routed to Sonar whatever its state
    /// </summary>
    /// <param name="processId">The id of the process</param>
    /// <returns>A list of <see cref="RoutedProcess"/></returns>
    IEnumerable<RoutedProcess> GetRoutedProcessesById(int processId);
    
    /// <summary>
    /// Get an audio process that is routed to Sonar and is active
    /// </summary>
    /// <param name="processId">The id of the process</param>
    /// <returns><see cref="RoutedProcess"/></returns>
    IEnumerable<RoutedProcess> GetActiveRoutedProcessesById(int processId);
    
    /// <summary>
    /// Route an audio process to a specific <see cref="Channel"/>
    /// </summary>
    /// <param name="processId">The id of the process</param>
    /// <param name="channel"><see cref="Channel"/></param>
    void RouteProcessToChannel(int processId, Channel channel);
    
    /// <summary>
    /// Route an audio process to a specific <see cref="Channel"/>
    /// </summary>
    /// <param name="process"><see cref="RoutedProcess"/></param>
    /// <param name="channel"><see cref="Channel"/></param>
    void RouteProcessToChannel(RoutedProcess process, Channel channel);
}