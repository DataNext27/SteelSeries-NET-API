namespace SteelSeriesAPI.Sonar.Enums;

public enum Mix
{
    MONITORING,
    STREAM
}

public static class MixExtensions
{
    private static readonly Dictionary<Mix, string> MixMap = new Dictionary<Mix, string>
    {
        { Mix.MONITORING, "monitoring" },
        { Mix.STREAM, "streaming" }
    };

    public static string ToDictKey(this Mix mix)
    {
        return MixMap[mix];
    }
    
    public static Mix? FromDictKey(string jsonKey)
    {
        foreach (var pair in MixMap)
        {
            if (pair.Value == jsonKey)
            {
                return pair.Key;
            }
        }

        return null;
    }
}