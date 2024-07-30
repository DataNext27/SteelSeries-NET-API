using SteelSeriesAPI.Sonar.Enums;

namespace SteelSeriesAPI.Sonar.Models;

public record RoutedProcess(string Id, string ProcessName, int PId, RoutedProcessState State, string DisplayName);