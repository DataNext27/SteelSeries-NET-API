using SteelSeriesAPI.Sonar.Enums;

namespace SteelSeriesAPI.Sonar.Events;

public class SonarRedirectionStateEvent : EventArgs
{
    // /streamRedirections/monitoring/redirections/chatRender/isEnabled/true
    
    public bool State { get; set; }
    
    public Device Device { get; set; }
    public Channel Channel { get; set; }
}