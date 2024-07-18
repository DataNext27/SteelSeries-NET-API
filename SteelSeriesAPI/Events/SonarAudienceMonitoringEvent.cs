namespace SteelSeriesAPI.Events;

public class SonarAudienceMonitoringEvent : EventArgs
{
    // /streamRedirections/isStreamMonitoringEnabled/bool
    public bool AudienceMonitoringState { get; set; }
}