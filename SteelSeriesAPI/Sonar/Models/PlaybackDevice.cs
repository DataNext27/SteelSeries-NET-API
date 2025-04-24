using System.Collections;
using SteelSeriesAPI.Sonar.Enums;

namespace SteelSeriesAPI.Sonar.Models;

public record PlaybackDevice(string Id, string Name, DataFlow DataFlow, List<Channel>? AssociatedClassicDevices, ArrayList? AssociatedStreamDevices);