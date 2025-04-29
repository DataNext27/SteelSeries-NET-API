namespace SteelSeriesAPI.Sonar.Enums;

public enum DataFlow
{
    INPUT,
    OUTPUT
}

public static class DataFlowExtensions
{
    private static readonly Dictionary<DataFlow, string> DataFlowMap = new Dictionary<DataFlow, string>
    {
        { DataFlow.INPUT, "capture" },
        { DataFlow.OUTPUT, "render" }
    };

    public static string ToDictKey(this DataFlow dataFlow)
    {
        return DataFlowMap[dataFlow];
    }
    
    public static DataFlow? FromDictKey(string jsonKey)
    {
        foreach (var pair in DataFlowMap)
        {
            if (pair.Value == jsonKey)
            {
                return pair.Key;
            }
        }

        return null;
    }
}