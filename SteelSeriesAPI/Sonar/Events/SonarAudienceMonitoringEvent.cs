namespace SteelSeriesAPI.Sonar.Events;

public class SonarAudienceMonitoringEvent : EventArgs
{
    // /streamRedirections/isStreamMonitoringEnabled/true
    public bool NewState { get; set; }
}