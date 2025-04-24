namespace SteelSeriesAPI.Sonar.Events;

public class SonarChatMixEvent : EventArgs
{
    // /chatMix?balance=0
    public double Balance { get; set; }
}