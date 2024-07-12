using SteelSeriesAPI.Sonar.Enums;

namespace SteelSeriesAPI.Sonar.Models;

public record RedirectionDevice(string Id, string Name, Direction DataFlow);