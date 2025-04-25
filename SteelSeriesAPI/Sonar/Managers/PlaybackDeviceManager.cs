using System.Collections;
using SteelSeriesAPI.Sonar.Interfaces.Managers;
using SteelSeriesAPI.Sonar.Enums;
using SteelSeriesAPI.Sonar.Http;
using SteelSeriesAPI.Sonar.Models;

using System.Text.Json;
using SteelSeriesAPI.Sonar.Exceptions;

namespace SteelSeriesAPI.Sonar.Managers;

public class PlaybackDeviceManager : IPlaybackDeviceManager
{
    public IEnumerable<PlaybackDevice> GetPlaybackDevices(DataFlow _dataFlow)
    {
        JsonDocument audioDevices = new HttpFetcher().Provide("audioDevices");
        // JsonDocument classicRedirections = new HttpProvider("classicRedirections").Provide();
        // JsonDocument streamRedirections = new HttpProvider("streamRedirections").Provide();
    
        foreach (var element in audioDevices.RootElement.EnumerateArray())
        {
            if (element.GetProperty("role").GetString() != "none")
            {
                continue;
            }
            
            string id = element.GetProperty("id").GetString();
            string name = element.GetProperty("friendlyName").GetString();
            string dataFlow = element.GetProperty("dataFlow").GetString();
            List<Channel> associatedClassicChannels = new List<Channel>();
            ArrayList associatedStreamChannels = new ArrayList();
            
            GetAssociatedChannels(id, associatedClassicChannels, associatedStreamChannels);
    
            if (dataFlow == _dataFlow.ToDictKey())
            {
                yield return new PlaybackDevice(id, name, (DataFlow)DataFlowExtensions.FromDictKey(dataFlow), associatedClassicChannels, associatedStreamChannels);
            }
        }
    }

    public PlaybackDevice GetPlaybackDevice(string deviceId)
    {
        try
        {
            JsonElement device = new HttpFetcher().Provide("audioDevices/" + deviceId).RootElement;
            
            string id = device.GetProperty("id").GetString();
            string name = device.GetProperty("friendlyName").GetString();
            string dataFlow = device.GetProperty("dataFlow").GetString();
            List<Channel> associatedClassicChannels = new List<Channel>();
            ArrayList associatedStreamChannels = new ArrayList();
        
            GetAssociatedChannels(deviceId, associatedClassicChannels, associatedStreamChannels);
        
            return new PlaybackDevice(id, name, (DataFlow)DataFlowExtensions.FromDictKey(dataFlow), associatedClassicChannels, associatedStreamChannels);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception("Can't get any device from this Id, maybe the device doesn't exist or its Id changed.");
        }
    }

    public PlaybackDevice GetClassicPlaybackDevice(Channel channel)
    {
        if (channel == Channel.MASTER)
        {
            throw new MasterChannelNotSupportedException();
        }
        
        JsonDocument classicRedirections = new HttpFetcher().Provide("classicRedirections");
        JsonElement cRedirections = default;
    
        foreach (var element in classicRedirections.RootElement.EnumerateArray())
        {
            if (element.GetProperty("id").GetString() == channel.ToDictKey(ChannelMapChoice.ChannelDict))
            {
                cRedirections = element;
                break;
            }
        }
        
        string deviceId = cRedirections.GetProperty("deviceId").GetString();
        List<Channel> associatedClassicChannels = new List<Channel>();
        ArrayList associatedStreamChannels = new ArrayList();
        
        GetAssociatedChannels(deviceId, associatedClassicChannels, associatedStreamChannels);
    
        JsonDocument audioDevice = new HttpFetcher().Provide("audioDevices/" + deviceId);
    
        string name = audioDevice.RootElement.GetProperty("friendlyName").GetString();
        DataFlow dataFlow = (DataFlow)DataFlowExtensions.FromDictKey(audioDevice.RootElement.GetProperty("dataFlow").GetString());
    
        return new PlaybackDevice(deviceId, name, dataFlow, associatedClassicChannels, associatedStreamChannels);
    }

    public PlaybackDevice GetStreamerPlaybackDevice(Mix mix)
    {
        JsonDocument streamRedirections = new HttpFetcher().Provide("streamRedirections");
        JsonElement sRedirections = default;
    
        foreach (var element in streamRedirections.RootElement.EnumerateArray())
        {
            if (element.GetProperty("streamRedirectionId").GetString() == mix.ToDictKey())
            {
                sRedirections = element;
                break;
            }
        }
        
        string deviceId = sRedirections.GetProperty("deviceId").GetString();
        List<Channel> associatedClassicChannels = new List<Channel>();
        ArrayList associatedStreamChannels = new ArrayList();

        GetAssociatedChannels(deviceId, associatedClassicChannels, associatedStreamChannels);
    
        JsonDocument audioDevice = new HttpFetcher().Provide("audioDevices/" + deviceId);
    
        string name = audioDevice.RootElement.GetProperty("friendlyName").GetString();
        DataFlow dataFlow = (DataFlow)DataFlowExtensions.FromDictKey(audioDevice.RootElement.GetProperty("dataFlow").GetString());
    
        return new PlaybackDevice(deviceId, name, dataFlow, associatedClassicChannels, associatedStreamChannels);
    }

    public PlaybackDevice GetStreamerPlaybackDevice(Channel channel = Channel.MIC)
    {
        if (channel != Channel.MIC)
        {
            throw new MicChannelSupportOnlyException();
        }
        
        JsonDocument streamRedirections = new HttpFetcher().Provide("streamRedirections");
        JsonElement sRedirections = default;
    
        foreach (var element in streamRedirections.RootElement.EnumerateArray())
        {
            if (element.GetProperty("streamRedirectionId").GetString() == channel.ToDictKey(ChannelMapChoice.ChannelDict))
            {
                sRedirections = element;
                break;
            }
        }
        
        string deviceId = sRedirections.GetProperty("deviceId").GetString();
        List<Channel> associatedClassicChannels = new List<Channel>();
        ArrayList associatedStreamChannels = new ArrayList();
        
        GetAssociatedChannels(deviceId, associatedClassicChannels, associatedStreamChannels);
    
        JsonDocument audioDevice = new HttpFetcher().Provide("audioDevices/" + deviceId);
    
        string name = audioDevice.RootElement.GetProperty("friendlyName").GetString();
        DataFlow dataFlow = (DataFlow)DataFlowExtensions.FromDictKey(audioDevice.RootElement.GetProperty("dataFlow").GetString());
    
        return new PlaybackDevice(deviceId, name, dataFlow, associatedClassicChannels, associatedStreamChannels);
    }
    
    private void GetAssociatedChannels(string deviceId, List<Channel> associatedClassicDevices, ArrayList associatedStreamDevices)
    {
        JsonDocument classicRedirections = new HttpFetcher().Provide("classicRedirections");
        JsonDocument streamRedirections = new HttpFetcher().Provide("streamRedirections");

        foreach (var element in classicRedirections.RootElement.EnumerateArray())
        {
            if (element.GetProperty("deviceId").GetString() == deviceId)
            {
                associatedClassicDevices.Add((Channel)ChannelExtensions.FromDictKey(element.GetProperty("id").GetString(), ChannelMapChoice.ChannelDict));
            }
        }
        
        foreach (var element in streamRedirections.RootElement.EnumerateArray())
        {
            if (element.GetProperty("deviceId").GetString() == deviceId)
            {
                if (element.GetProperty("streamRedirectionId").GetString() == "mic")
                {
                    associatedStreamDevices.Add((Channel)ChannelExtensions.FromDictKey(element.GetProperty("streamRedirectionId").GetString(), ChannelMapChoice.ChannelDict));
                }
                else
                {
                    associatedStreamDevices.Add((Mix)MixExtensions.FromDictKey(element.GetProperty("streamRedirectionId").GetString()));
                }
            }
        }
    }

    public void SetClassicPlaybackDevice(string deviceId, Channel channel)
    {
        new HttpFetcher().Put("classicRedirections/" + channel.ToDictKey(ChannelMapChoice.ChannelDict) +"/deviceId/" + deviceId);
    }

    public void SetStreamerPlaybackDevice(string deviceId, Mix mix)
    {
        new HttpFetcher().Put("streamRedirections/" + mix.ToDictKey() +"/deviceId/" + deviceId);
    }

    public void SetStreamerPlaybackDevice(string deviceId, Channel channel = Channel.MIC)
    {
        if (channel != Channel.MIC)
        {
            throw new MicChannelSupportOnlyException();
        }
        
        new HttpFetcher().Put("streamRedirections/" + channel.ToDictKey(ChannelMapChoice.ChannelDict) +"/deviceId/" + deviceId);
    }

    public void SetClassicPlaybackDevice(PlaybackDevice playbackDevice, Channel channel)
    {
        SetClassicPlaybackDevice(playbackDevice.Id, channel);
    }

    public void SetStreamerPlaybackDevice(PlaybackDevice playbackDevice, Mix mix)
    {
        SetStreamerPlaybackDevice(playbackDevice.Id, mix);
    }

    public void SetStreamerPlaybackDevice(PlaybackDevice playbackDevice, Channel channel = Channel.MIC)
    {
        SetStreamerPlaybackDevice(playbackDevice.Id, channel);
    }
}