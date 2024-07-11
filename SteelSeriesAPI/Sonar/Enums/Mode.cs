namespace SteelSeriesAPI.Sonar.Enums;

public enum Mode
{
    Classic,
    Streamer
}

public enum ModeMapChoice
{
    StreamerDict,
    StreamDict
}

public static class ModeExtensions
{
    private static readonly Dictionary<Mode, string> PrimaryModeMap = new Dictionary<Mode, string>
    {
        { Mode.Classic, "classic" },
        { Mode.Streamer, "streamer" }
    };
    
    private static readonly Dictionary<Mode, string> SecondaryModeMap = new Dictionary<Mode, string>
    {
        { Mode.Classic, "classic" },
        { Mode.Streamer, "stream" }
    };

    public static string ToDictKey(this Mode mode, ModeMapChoice context = ModeMapChoice.StreamerDict)
    {
        return context switch
        {
            ModeMapChoice.StreamerDict => PrimaryModeMap.ContainsKey(mode) ? PrimaryModeMap[mode] : null,
            ModeMapChoice.StreamDict => SecondaryModeMap.ContainsKey(mode) ? SecondaryModeMap[mode] : null,
            _ => null
        };
    }
    
    public static Mode? FromDictKey(string jsonKey, ModeMapChoice context = ModeMapChoice.StreamerDict)
    {
        var map = context switch
        {
            ModeMapChoice.StreamerDict => PrimaryModeMap,
            ModeMapChoice.StreamDict => SecondaryModeMap,
            _ => null
        };

        if (map != null)
        {
            foreach (var pair in map)
            {
                if (pair.Value == jsonKey)
                {
                    return pair.Key;
                }
            }
        }

        return null;
    }
}