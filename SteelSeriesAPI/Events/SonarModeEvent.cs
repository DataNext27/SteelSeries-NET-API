using SteelSeriesAPI.Sonar.Enums;

namespace SteelSeriesAPI.Events;

public class SonarModeEvent : EventArgs
{
    // /mode/stream
    
    public Mode NewMode { get; set; }
}