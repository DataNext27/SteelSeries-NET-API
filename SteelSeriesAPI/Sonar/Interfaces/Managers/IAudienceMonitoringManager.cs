namespace SteelSeriesAPI.Sonar.Interfaces.Managers;

/// <summary>
/// Manage the Audience Monitoring feature of the streamer mode
/// </summary>
public interface IAudienceMonitoringManager
{
    /// <summary>
    /// Get the current state of the Audience Monitoring
    /// </summary>
    /// <returns>The current state, un/muted</returns>
    bool GetState();
    
    /// <summary>
    /// Activate or deactivate Audience Monitoring<br/>
    /// Listen to what your audience hear
    /// </summary>
    /// <param name="newState">The new state, un/muted</param>
    void SetState(bool newState);
}