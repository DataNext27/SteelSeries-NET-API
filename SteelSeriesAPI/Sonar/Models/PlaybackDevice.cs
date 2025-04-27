using SteelSeriesAPI.Sonar.Enums;
using System.Collections;

namespace SteelSeriesAPI.Sonar.Models;

public record PlaybackDevice(string Id, string Name, DataFlow DataFlow, List<Tuple<Channel, Mode>> Channels, List<Mix> Mixes);