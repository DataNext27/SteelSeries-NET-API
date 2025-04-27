using SteelSeriesAPI.Sonar.Interfaces.Managers;
using SteelSeriesAPI.Sonar.Enums;
using SteelSeriesAPI.Sonar.Http;
using SteelSeriesAPI.Sonar.Models;

using System.Text.Json;
using SteelSeriesAPI.Sonar.Exceptions;

namespace SteelSeriesAPI.Sonar.Managers;

internal class ConfigurationManager : IConfigurationManager
{
    public IEnumerable<SonarAudioConfiguration> GetAllAudioConfigurations()
    {
        JsonElement configs = new Fetcher().Provide("configs").RootElement;

        foreach (JsonElement config in configs.EnumerateArray())
        {
            string device = config.GetProperty("virtualAudioDevice").GetString()!;
            string id = config.GetProperty("id").GetString()!;
            string name = config.GetProperty("name").GetString()!;
            
            yield return new SonarAudioConfiguration(id, name, (Channel)ChannelExtensions.FromDictKey(device)!);
        }
    }

    public IEnumerable<SonarAudioConfiguration> GetAudioConfigurations(Channel channel)
    {
        if (channel == Channel.MASTER)
        {
            throw new MasterChannelNotSupportedException();
        }
        
        JsonElement configs = new Fetcher().Provide("configs").RootElement;

        foreach (JsonElement config in configs.EnumerateArray())
        {
            string device = config.GetProperty("virtualAudioDevice").GetString()!;
            if (device == channel.ToDictKey())
            {
                string id = config.GetProperty("id").GetString()!;
                string name = config.GetProperty("name").GetString()!;
                
                yield return new SonarAudioConfiguration(id, name, (Channel)ChannelExtensions.FromDictKey(device)!);
            }
        }
    }
    
    public SonarAudioConfiguration GetSelectedAudioConfiguration(Channel channel)
    {
        if (channel == Channel.MASTER)
        {
            throw new MasterChannelNotSupportedException();
        }
        
        JsonElement selectedConfigs = new Fetcher().Provide("configs/selected").RootElement;

        foreach (JsonElement config in selectedConfigs.EnumerateArray())
        {
            var device = config.GetProperty("virtualAudioDevice").GetString()!;
            if (device == channel.ToDictKey())
            {
                string id = config.GetProperty("id").GetString()!;
                string name = config.GetProperty("name").GetString()!;

                return new SonarAudioConfiguration(id, name, (Channel)ChannelExtensions.FromDictKey(device)!);
            }
        }

        throw new ChannelNotFoundException();
    }

    public SonarAudioConfiguration GetAudioConfiguration(string configId)
    {
        JsonElement configs = new Fetcher().Provide("configs").RootElement;

        foreach (JsonElement config in configs.EnumerateArray())
        {
            string id = config.GetProperty("id").GetString()!;
            if (id == configId)
            {
                string device = config.GetProperty("virtualAudioDevice").GetString()!;
                string name = config.GetProperty("name").GetString()!;
                
                return new SonarAudioConfiguration(id, name, (Channel)ChannelExtensions.FromDictKey(device)!);
            }
        }
        
        throw new ConfigNotFoundException($"No audio configuration found with this id: {configId}");
    }

    public void SetConfig(string configId)
    {
        if (string.IsNullOrEmpty(configId)) throw new ConfigNotFoundException("Id can't be null or empty");
        
        JsonElement configs = new Fetcher().Provide("configs").RootElement;

        foreach (JsonElement config in configs.EnumerateArray())
        {
            string id = config.GetProperty("id").GetString()!;
            if (id == configId)
            {
                new Fetcher().Put("configs/" + configId + "/select");
                return;
            }
        }
        
        throw new ConfigNotFoundException($"No audio configuration found with this id: {configId}");
    }

    public void SetConfig(SonarAudioConfiguration config)
    {
        SetConfig(config.Id);
    }

    public void SetConfigByName(Channel channel, string name)
    {
        var configs = GetAudioConfigurations(channel).ToList();
        foreach (var config in configs)
        {
            if (config.Name == name)
            {
                SetConfig(config.Id);
                break;
            }
        }
    }
}