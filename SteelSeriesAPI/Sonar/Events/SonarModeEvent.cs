using SteelSeriesAPI.Sonar.Enums;

namespace SteelSeriesAPI.Sonar.Events;

public class SonarModeEvent : EventArgs
{
    // /mode/stream
    
    public Mode NewMode { get; set; }
}