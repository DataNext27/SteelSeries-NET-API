namespace SteelSeriesAPI.Sonar.Enums;

public enum Direction
{
    Input,
    Output
}

public static class DirectionExtensions
{
    private static readonly Dictionary<Direction, string> DirectionMap = new Dictionary<Direction, string>
    {
        { Direction.Input, "capture" },
        { Direction.Output, "render" }
    };

    public static string ToDictKey(this Direction direction)
    {
        return DirectionMap[direction];
    }
    
    public static Direction? FromDictKey(string jsonKey)
    {
        foreach (var pair in DirectionMap)
        {
            if (pair.Value == jsonKey)
            {
                return pair.Key;
            }
        }

        return null;
    }
}