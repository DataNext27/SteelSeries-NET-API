namespace SteelSeriesAPI.Events;

public class SonarConfigEvent : EventArgs
{
    // /configs/e6979db3-3e00-4399-b58c-6f026c9ef6ba/select
    // /configs <--- Error
    public string ConfigId { get; set; }
}