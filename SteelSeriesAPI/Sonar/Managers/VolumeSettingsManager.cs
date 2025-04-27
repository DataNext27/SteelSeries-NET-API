using SteelSeriesAPI.Sonar.Interfaces.Managers;
using SteelSeriesAPI.Sonar.Enums;
using SteelSeriesAPI.Sonar.Http;

using System.Globalization;
using System.Text.Json;

namespace SteelSeriesAPI.Sonar.Managers;

internal class VolumeSettingsManager : IVolumeSettingsManager
{
    // volume = 0,00000000 <-- 8 decimal max
    public double GetVolume(Channel channel)
    {
        JsonDocument volumeSettings = new Fetcher().Provide("volumeSettings/classic/");

        if (channel == Channel.MASTER)
            return volumeSettings.RootElement.GetProperty("masters").GetProperty("classic").GetProperty("volume").GetDouble();
        return volumeSettings.RootElement.GetProperty("devices").GetProperty(channel.ToDictKey()).GetProperty("classic").GetProperty("volume").GetDouble();
    }

    public double GetVolume(Channel channel, Mix mix)
    {
        JsonDocument volumeSettings = new Fetcher().Provide("volumeSettings/streamer/");

        if (channel == Channel.MASTER)
            return volumeSettings.RootElement.GetProperty("masters").GetProperty("stream").GetProperty(mix.ToDictKey()).GetProperty("volume").GetDouble();
        return volumeSettings.RootElement.GetProperty("devices").GetProperty(channel.ToDictKey()).GetProperty("stream").GetProperty(mix.ToDictKey()).GetProperty("volume").GetDouble();
    }

    public bool GetMute(Channel channel)
    {
        JsonDocument volumeSettings = new Fetcher().Provide("volumeSettings/classic/");

        if (channel == Channel.MASTER)
            return volumeSettings.RootElement.GetProperty("masters").GetProperty("classic").GetProperty("muted").GetBoolean();
        return volumeSettings.RootElement.GetProperty("devices").GetProperty(channel.ToDictKey()).GetProperty("classic").GetProperty("muted").GetBoolean();
    }

    public bool GetMute(Channel channel, Mix mix)
    {
        JsonDocument volumeSettings = new Fetcher().Provide("volumeSettings/streamer/");

        if (channel == Channel.MASTER)
            return volumeSettings.RootElement.GetProperty("masters").GetProperty("stream").GetProperty(mix.ToDictKey()).GetProperty("muted").GetBoolean();
        return volumeSettings.RootElement.GetProperty("devices").GetProperty(channel.ToDictKey()).GetProperty("stream").GetProperty(mix.ToDictKey()).GetProperty("muted").GetBoolean();
    }

    public void SetVolume(double volume, Channel channel)
    {
        string vol = volume.ToString("0.00", CultureInfo.InvariantCulture);
        new Fetcher().Put("volumeSettings/classic/" + channel.ToDictKey(ChannelMapChoice.HttpDict) + "/Volume/" + vol);
    }

    public void SetVolume(double volume, Channel channel, Mix mix)
    {
        string vol = volume.ToString("0.00", CultureInfo.InvariantCulture);
        new Fetcher().Put("volumeSettings/streamer/" + mix.ToDictKey() + "/" + channel.ToDictKey(ChannelMapChoice.HttpDict) + "/volume/" + vol);
    }

    public void SetMute(bool mute, Channel channel)
    {
        new Fetcher().Put("volumeSettings/classic/" + channel.ToDictKey(ChannelMapChoice.HttpDict) + "/Mute/" + mute);
    }

    public void SetMute(bool mute, Channel channel, Mix mix)
    {
        new Fetcher().Put("volumeSettings/streamer/" + mix.ToDictKey() + "/" + channel.ToDictKey(ChannelMapChoice.HttpDict) + "/isMuted/" + mute);
    }
}