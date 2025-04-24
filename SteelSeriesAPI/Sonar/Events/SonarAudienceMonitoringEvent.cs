namespace SteelSeriesAPI.Sonar.Events;

public class SonarAudienceMonitoringEvent : EventArgs
{
    // /streamRedirections/isStreamMonitoringEnabled/bool
    public bool AudienceMonitoringState { get; set; }
}