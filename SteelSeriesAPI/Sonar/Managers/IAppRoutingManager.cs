using SteelSeriesAPI.Sonar.Enums;
using SteelSeriesAPI.Sonar.Models;

namespace SteelSeriesAPI.Sonar.Managers;

/// <summary>Reads and controls which Sonar channel each application's audio is routed to.</summary>
public interface IAppRoutingManager
{
    /// <summary>Gets the routing state of every device, with their attached audio sessions.</summary>
    /// <param name="ct">A token to cancel the operation.</param>
    Task<IReadOnlyList<DeviceRouting>> GetRoutingsAsync(CancellationToken ct = default);

    /// <summary>
    /// Routes an application's output audio to a Sonar channel.
    /// Resolves the channel's virtual device, then applies the routing.
    /// </summary>
    /// <param name="processId">The application's process id (volatile: resolve it at call time).</param>
    /// <param name="channel">The channel to route the application to.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <exception cref="SonarResponseException">No virtual device found for the channel.</exception>
    Task RouteAppAsync(int processId, Channel channel, CancellationToken ct = default);

    /// <summary>Routes an application to an explicit device. Low-level variant.</summary>
    /// <param name="processId">The application's process id.</param>
    /// <param name="deviceId">The target device identifier.</param>
    /// <param name="dataFlow">The direction of the routing (render for app output).</param>
    /// <param name="ct">A token to cancel the operation.</param>
    Task RouteAppAsync(int processId, string deviceId, AudioDataFlow dataFlow, CancellationToken ct = default);
}