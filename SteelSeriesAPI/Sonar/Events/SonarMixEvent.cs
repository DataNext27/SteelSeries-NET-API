using SteelSeriesAPI.Sonar.Enums;

namespace SteelSeriesAPI.Sonar.Events;

public class SonarMixEvent : EventArgs
{
    // /streamRedirections/monitoring/redirections/chatRender/isEnabled/true
    
    public bool NewState { get; set; }
    
    public Channel Channel { get; set; }
    
    public Mix Mix { get; set; }
}