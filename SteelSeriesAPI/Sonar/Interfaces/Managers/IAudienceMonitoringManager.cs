namespace SteelSeriesAPI.Sonar.Interfaces.Managers;

public interface IAudienceMonitoringManager
{
    /// <summary>
    /// Get the current state of the Audience Monitoring
    /// </summary>
    /// <returns>The current state, un/muted</returns>
    bool GetState();
    
    /// <summary>
    /// Listen to what your audience hear
    /// </summary>
    /// <param name="newState">The new state, un/muted</param>
    void SetState(bool newState);
}