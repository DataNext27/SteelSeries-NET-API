using System.Collections;
using SteelSeriesAPI.Sonar.Exceptions;
using SteelSeriesAPI.Sonar.Enums;
using SteelSeriesAPI.Sonar.Http;
using SteelSeriesAPI.Sonar.Interfaces.Managers;
using SteelSeriesAPI.Sonar.Models;

using System.Text.Json;

namespace SteelSeriesAPI.Sonar.Managers;

internal class PlaybackDeviceManager : IPlaybackDeviceManager
{
    public IEnumerable<PlaybackDevice> GetAllPlaybackDevices()
    {
        JsonElement audioDevices = new Fetcher().Provide("audioDevices").RootElement;

        foreach (JsonElement device in audioDevices.EnumerateArray())
        {
            if (device.GetProperty("role").GetString() == "none")
            {
                string id = device.GetProperty("id").GetString()!;
                string name = device.GetProperty("friendlyName").GetString()!;
                DataFlow dataFlow = (DataFlow)DataFlowExtensions.FromDictKey(device.GetProperty("dataFlow").GetString()!)!;
                List<Tuple<Channel, Mode>> channels = new List<Tuple<Channel, Mode>>();
                List<Mix> mixes = new List<Mix>();

                GetChannelsAndMixes(id, channels, mixes);
                
                yield return new PlaybackDevice(id, name, dataFlow, channels, mixes);
            }
        }
    }

    public IEnumerable<PlaybackDevice> GetOutputPlaybackDevices()
    {
        JsonElement audioDevices = new Fetcher().Provide("audioDevices").RootElement;

        foreach (JsonElement device in audioDevices.EnumerateArray())
        {
            if (device.GetProperty("role").GetString() == "none")
            {
                string dataFlow = device.GetProperty("dataFlow").GetString()!;
                if (dataFlow == "render")
                {
                    string id = device.GetProperty("id").GetString()!;
                    string name = device.GetProperty("friendlyName").GetString()!;
                    List<Tuple<Channel, Mode>> channels = new List<Tuple<Channel, Mode>>();
                    List<Mix> mixes = new List<Mix>();
                    
                    GetChannelsAndMixes(id, channels, mixes);
                    
                    yield return new PlaybackDevice(id, name, DataFlow.OUTPUT, channels, mixes);
                }
            }
        }
    }

    public IEnumerable<PlaybackDevice> GetInputPlaybackDevices()
    {
        JsonElement audioDevices = new Fetcher().Provide("audioDevices").RootElement;

        foreach (JsonElement device in audioDevices.EnumerateArray())
        {
            if (device.GetProperty("role").GetString() == "none")
            {
                string dataFlow = device.GetProperty("dataFlow").GetString()!;
                if (dataFlow == "capture")
                {
                    string id = device.GetProperty("id").GetString()!;
                    string name = device.GetProperty("friendlyName").GetString()!;
                    List<Tuple<Channel, Mode>> channels = new List<Tuple<Channel, Mode>>();
                    List<Mix> mixes = new List<Mix>();
                    
                    GetChannelsAndMixes(id, channels, mixes);
                
                    yield return new PlaybackDevice(id, name, DataFlow.INPUT, channels, mixes);
                }
            }
        }
    }

    public PlaybackDevice GetPlaybackDevice(Channel channel)
    {
        JsonElement classicRedirections = new Fetcher().Provide("classicRedirections").RootElement;
        JsonElement audioDevices = new Fetcher().Provide("audioDevices").RootElement;

        foreach (JsonElement redirection in classicRedirections.EnumerateArray())
        {
            if (redirection.GetProperty("id").GetString() == channel.ToDictKey(ChannelMapChoice.ChannelDict))
            {
                string deviceId = redirection.GetProperty("deviceId").GetString()!;

                if (string.IsNullOrEmpty(deviceId))
                {
                    throw new PlaybackDeviceNotFoundException("No device set on this channel");
                }

                foreach (JsonElement device in audioDevices.EnumerateArray())
                {
                    if (device.GetProperty("id").GetString() == deviceId)
                    {
                        string name = device.GetProperty("friendlyName").GetString()!;
                        DataFlow dataFlow = (DataFlow)DataFlowExtensions.FromDictKey(device.GetProperty("dataFlow").GetString()!)!;
                        List<Tuple<Channel, Mode>> channels = new List<Tuple<Channel, Mode>>();
                        List<Mix> mixes = new List<Mix>();
                        
                        GetChannelsAndMixes(deviceId, channels, mixes);
                        
                        return new PlaybackDevice(deviceId, name, dataFlow, channels, mixes);
                    }
                }
                
                throw new PlaybackDeviceNotFoundException("Could not find the device");
            }
        }

        throw new ChannelNotFoundException("Could not find the Channel");
    }

    public PlaybackDevice GetPlaybackDevice(Channel channel, Mode mode)
    {
        if (mode == Mode.CLASSIC)
        {
            return GetPlaybackDevice(channel);
        }

        if (mode == Mode.STREAMER && channel != Channel.MIC)
        {
            throw new ChannelNoStreamerSupportException();
        }
        
        JsonElement streamRedirections = new Fetcher().Provide("streamRedirections").RootElement;
        JsonElement audioDevices = new Fetcher().Provide("audioDevices").RootElement;
        
        foreach (JsonElement redirection in streamRedirections.EnumerateArray())
        {
            if (redirection.GetProperty("streamRedirectionId").GetString() == channel.ToDictKey(ChannelMapChoice.ChannelDict))
            {
                string deviceId = redirection.GetProperty("deviceId").GetString()!;
                
                if (string.IsNullOrEmpty(deviceId))
                {
                    throw new PlaybackDeviceNotFoundException("No device set on this channel");
                }

                foreach (JsonElement device in audioDevices.EnumerateArray())
                {
                    if (device.GetProperty("id").GetString() == deviceId)
                    {
                        string name = device.GetProperty("friendlyName").GetString()!;
                        DataFlow dataFlow = (DataFlow)DataFlowExtensions.FromDictKey(device.GetProperty("dataFlow").GetString()!)!;
                        List<Tuple<Channel, Mode>> channels = new List<Tuple<Channel, Mode>>();
                        List<Mix> mixes = new List<Mix>();
                        
                        GetChannelsAndMixes(deviceId, channels, mixes);
                        
                        return new PlaybackDevice(deviceId, name, dataFlow, channels, mixes);
                    }
                }
                
                throw new PlaybackDeviceNotFoundException("Could not find the device");
            }
        }
        
        throw new ChannelNotFoundException("Could not find the Channel");
    }

    public PlaybackDevice GetPlaybackDevice(Mix mix)
    {
        JsonElement streamRedirections = new Fetcher().Provide("streamRedirections").RootElement;
        JsonElement audioDevices = new Fetcher().Provide("audioDevices").RootElement;
        
        foreach (JsonElement redirection in streamRedirections.EnumerateArray())
        {
            if (redirection.GetProperty("streamRedirectionId").GetString() == mix.ToDictKey())
            {
                string deviceId = redirection.GetProperty("deviceId").GetString()!;
                
                if (string.IsNullOrEmpty(deviceId))
                {
                    throw new PlaybackDeviceNotFoundException("No device set on this channel");
                }

                foreach (JsonElement device in audioDevices.EnumerateArray())
                {
                    if (device.GetProperty("id").GetString() == deviceId)
                    {
                        string name = device.GetProperty("friendlyName").GetString()!;
                        DataFlow dataFlow = (DataFlow)DataFlowExtensions.FromDictKey(device.GetProperty("dataFlow").GetString()!)!;
                        List<Tuple<Channel, Mode>> channels = new List<Tuple<Channel, Mode>>();
                        List<Mix> mixes = new List<Mix>();
                        
                        GetChannelsAndMixes(deviceId, channels, mixes);
                        
                        return new PlaybackDevice(deviceId, name, dataFlow, channels, mixes);
                    }
                }
                
                throw new PlaybackDeviceNotFoundException("Could not find the device");
            }
        }
        
        throw new ChannelNotFoundException("Could not find the Channel");
    }

    public PlaybackDevice GetPlaybackDevice(string deviceId)
    {
        JsonElement audioDevices = new Fetcher().Provide("audioDevices").RootElement;

        foreach (JsonElement device in audioDevices.EnumerateArray())
        {
            if (device.GetProperty("id").GetString() == deviceId)
            {
                string name = device.GetProperty("friendlyName").GetString()!;
                DataFlow dataFlow = (DataFlow)DataFlowExtensions.FromDictKey(device.GetProperty("dataFlow").GetString()!)!;
                List<Tuple<Channel, Mode>> channels = new List<Tuple<Channel, Mode>>();
                List<Mix> mixes = new List<Mix>();
                
                GetChannelsAndMixes(deviceId, channels, mixes);
                
                return new PlaybackDevice(deviceId, name, dataFlow, channels, mixes);
            }
        }
        
        throw new PlaybackDeviceNotFoundException("Could not find the device");
    }

    private void GetChannelsAndMixes(string deviceId, List<Tuple<Channel, Mode>> channels, List<Mix> mixes)
    {
        JsonElement classicRedirections = new Fetcher().Provide("classicRedirections").RootElement;

        foreach (JsonElement redirection in classicRedirections.EnumerateArray())
        {
            if (redirection.GetProperty("deviceId").GetString() == deviceId)
            {
                channels.Add(new Tuple<Channel, Mode>((Channel)ChannelExtensions.FromDictKey(redirection.GetProperty("id").GetString()!, ChannelMapChoice.ChannelDict)!, Mode.CLASSIC));
            }
        }
        
        JsonElement streamRedirections = new Fetcher().Provide("streamRedirections").RootElement;

        foreach (JsonElement redirection in streamRedirections.EnumerateArray())
        {
            if (redirection.GetProperty("deviceId").GetString() == deviceId)
            {
                var id = redirection.GetProperty("streamRedirectionId").GetString()!;
                if (id == "mic")
                {
                    channels.Add(new Tuple<Channel, Mode>(Channel.MIC, Mode.STREAMER));
                }
                else
                {
                    mixes.Add((Mix)MixExtensions.FromDictKey(id)!);
                }
            }
        }
    }

    public void SetPlaybackDevice(string deviceId, Channel channel)
    {
        JsonElement audioDevices = new Fetcher().Provide("audioDevices").RootElement;

        foreach (JsonElement device in audioDevices.EnumerateArray())
        {
            if (device.GetProperty("id").GetString() == deviceId)
            {
                string dataFlow = device.GetProperty("dataFlow").GetString()!;
                if ((dataFlow == "render" && channel == Channel.MIC)
                    || (dataFlow == "capture" && channel != Channel.MIC))
                {
                    throw new PlaybackDeviceDataFlowException();
                }
                
                new Fetcher().Put("classicRedirections/" + channel.ToDictKey(ChannelMapChoice.ChannelDict) +"/deviceId/" + deviceId);
                return;
            }
        }
        
        throw new PlaybackDeviceNotFoundException("Could not find the device");
    }

    public void SetPlaybackDevice(string deviceId, Channel channel, Mode mode)
    {
        if (mode == Mode.CLASSIC)
        {
            SetPlaybackDevice(deviceId, channel);
        }
        
        if (mode == Mode.STREAMER && channel != Channel.MIC)
        {
            throw new ChannelNoStreamerSupportException();
        }
        
        JsonElement audioDevices = new Fetcher().Provide("audioDevices").RootElement;

        foreach (JsonElement device in audioDevices.EnumerateArray())
        {
            if (device.GetProperty("id").GetString() == deviceId)
            {
                string dataFlow = device.GetProperty("dataFlow").GetString()!;
                if (dataFlow == "capture" && channel != Channel.MIC)
                {
                    throw new PlaybackDeviceDataFlowException();
                }
                
                new Fetcher().Put("streamRedirections/" + channel.ToDictKey(ChannelMapChoice.ChannelDict) +"/deviceId/" + deviceId);
                return;
            }
        }
        
        throw new PlaybackDeviceNotFoundException("Could not find the device");
    }

    public void SetPlaybackDevice(string deviceId, Mix mix)
    {
        JsonElement audioDevices = new Fetcher().Provide("audioDevices").RootElement;

        foreach (JsonElement device in audioDevices.EnumerateArray())
        {
            if (device.GetProperty("id").GetString() == deviceId)
            {
                string dataFlow = device.GetProperty("dataFlow").GetString()!;
                if (dataFlow == "capture")
                {
                    throw new PlaybackDeviceDataFlowException();
                }
                
                new Fetcher().Put("streamRedirections/" + mix.ToDictKey() +"/deviceId/" + deviceId);
                return;
            }
        }
        
        throw new PlaybackDeviceNotFoundException("Could not find the device");
    }

    public void SetPlaybackDevice(PlaybackDevice device, Channel channel)
    {
        SetPlaybackDevice(device.Id, channel);
    }

    public void SetPlaybackDevice(PlaybackDevice device, Channel channel, Mode mode)
    {
        SetPlaybackDevice(device.Id, channel, mode);
    }

    public void SetPlaybackDevice(PlaybackDevice device, Mix mix)
    {
        SetPlaybackDevice(device.Id, mix);
    }
}