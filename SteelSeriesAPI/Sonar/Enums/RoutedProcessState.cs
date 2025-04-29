namespace SteelSeriesAPI.Sonar.Enums;

public enum RoutedProcessState
{
    ACTIVE,
    INACTIVE,
    EXPIRED
}

public static class RoutedProcessStateExtensions
{
    private static readonly Dictionary<RoutedProcessState, string> RoutedProcessStateMap = new Dictionary<RoutedProcessState, string>
    {
        { RoutedProcessState.ACTIVE, "active" },
        { RoutedProcessState.INACTIVE, "inactive" },
        { RoutedProcessState.EXPIRED, "expired" },
    };

    public static string ToDictKey(this RoutedProcessState state)
    {
        return RoutedProcessStateMap[state];
    }
    
    public static RoutedProcessState? FromDictKey(string jsonKey)
    {
        foreach (var pair in RoutedProcessStateMap)
        {
            if (pair.Value == jsonKey)
            {
                return pair.Key;
            }
        }
        
        return null;
    }
}