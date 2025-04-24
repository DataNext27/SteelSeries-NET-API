using SteelSeriesAPI.Sonar.Enums;

namespace SteelSeriesAPI.Sonar.Events;

public class SonarMuteEvent : EventArgs
{
    // /volumeSettings/classic/game/Mute/bool
    // /volumeSettings/streamer/monitoring/game/isMuted/bool
    
    public bool Muted { get; set; }
    
    public Mode Mode { get; set; }
    
    public Device Device { get; set; }
    public Channel? Channel { get; set; }
}