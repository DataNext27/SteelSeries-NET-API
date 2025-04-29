using SteelSeriesAPI.Sonar.Enums;

namespace SteelSeriesAPI.Sonar.Models;

public record RoutedProcess
{
    public int ProcessId { get; init; }
    public string ProcessName { get; init; }
    public string DisplayName { get; init; }
    public RoutedProcessState State { get; internal set; }
    public Channel Channel { get; init; }
    public string ProcessPath { get; init; }

    public RoutedProcess(int processId, string processName, string displayName, RoutedProcessState state, Channel channel, string processPath)
    {
        ProcessId = processId;
        ProcessName = processName;
        DisplayName = displayName;
        State = state;
        Channel = channel;
        ProcessPath = processPath;
    }
};