using SteelSeriesAPI.Sonar.Enums;

namespace SteelSeriesAPI.Sonar.Models;

public record SonarAudioConfiguration(string Id, string Name, Channel AssociatedChannel);