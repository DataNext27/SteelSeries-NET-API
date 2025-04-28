using SteelSeriesAPI.Sonar.Enums;

namespace SteelSeriesAPI.Sonar.Models;

public record RoutedProcess(int ProcessId, string ProcessName, string DisplayName, RoutedProcessState State, Channel Channel, string ProcessPath);