using SteelSeriesAPI.Sonar.Enums;
using SteelSeriesAPI.Sonar.Http;
using SteelSeriesAPI.Sonar.Exceptions;

using System.Text.Json;

namespace SteelSeriesAPI.Sonar.Events;

public class SonarRoutedProcessEvent : EventArgs
{
    // /AudioDeviceRouting/render/%7B0.0.0.00000000%7D.%7Beb78557a-9882-4205-8014-ad9384173901%7D/4476
    // /AudioDeviceRouting/capture/%7B0.0.1.00000000%7D.%7B989ad130-4b1f-4828-a85a-7aef7fd362b7%7D/4476
    
    public int ProcessId { get; init; }
    
    public Channel NewChannel { get; init; }

    internal SonarRoutedProcessEvent(string deviceId)
    {
        NewChannel = DeviceIdToChannel(deviceId);
    }

    private Channel DeviceIdToChannel(string deviceId)
    {
        JsonElement audioDeviceRouting = new Fetcher().Provide("AudioDeviceRouting").RootElement;

        foreach (JsonElement device in audioDeviceRouting.EnumerateArray())
        {
            if (device.GetProperty("role").GetString() != "none")
            {
                if (device.GetProperty("deviceId").GetString() == deviceId)
                {
                    return (Channel)ChannelExtensions.FromDictKey(device.GetProperty("role").GetString()!)!;
                }
            }
        }

        throw new RoutedProcessNotFoundException("Event error: Could not find the channel");
    }
}