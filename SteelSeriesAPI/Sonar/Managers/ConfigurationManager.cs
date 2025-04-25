using SteelSeriesAPI.Sonar.Interfaces.Managers;
using SteelSeriesAPI.Sonar.Enums;
using SteelSeriesAPI.Sonar.Http;
using SteelSeriesAPI.Sonar.Models;

using System.Text.Json;

namespace SteelSeriesAPI.Sonar.Managers;

public class ConfigurationManager : IConfigurationManager
{
    public IEnumerable<SonarAudioConfiguration> GetAllAudioConfigurations()
    {
        JsonDocument configs = new HttpFetcher().Provide("configs");

        foreach (var element in configs.RootElement.EnumerateArray())
        {
            string vDevice = element.GetProperty("virtualAudioDevice").GetString();
            string id = element.GetProperty("id").GetString();
            string name = element.GetProperty("name").GetString();
            
            yield return new SonarAudioConfiguration(id, name, (Channel)ChannelExtensions.FromDictKey(vDevice));
        }
    }

    public IEnumerable<SonarAudioConfiguration> GetAudioConfigurations(Channel channel)
    {
        if (channel == Channel.MASTER)
        {
            throw new Exception("Can't get audio configurations for master");
        }
        
        IEnumerable<SonarAudioConfiguration> configs = GetAllAudioConfigurations();
        List<SonarAudioConfiguration> channelConfigs = new List<SonarAudioConfiguration>();
        
        foreach (var config in configs)
        {
            if (config.AssociatedChannel == channel)
            {
                channelConfigs.Add(config);
            }
        }

        return channelConfigs.OrderBy(s => s.Name);
    }

    public SonarAudioConfiguration GetAudioConfiguration(string configId)
    {
        IEnumerable<SonarAudioConfiguration> configs = GetAllAudioConfigurations();
        SonarAudioConfiguration sonarConfig = null;
        
        foreach (var config in configs)
        {
            if (config.Id == configId)
            {
                sonarConfig = config;
                break;
            }
        }

        return sonarConfig;
    }

    public SonarAudioConfiguration GetSelectedAudioConfiguration(Channel channel)
    {
        if (channel == Channel.MASTER)
        {
            throw new Exception("Can't get audio configuration for master");
        }

        JsonDocument selectedConfigs = new HttpFetcher().Provide("configs/selected");
        JsonElement sConfig = default;

        foreach (var config in selectedConfigs.RootElement.EnumerateArray())
        {
            if (config.GetProperty("virtualAudioDevice").GetString() == channel.ToDictKey())
            {
                sConfig = config;
                break;
            }
        }

        string id = sConfig.GetProperty("id").GetString();
        string name = sConfig.GetProperty("name").GetString();
        string vDevice = sConfig.GetProperty("virtualAudioDevice").GetString();

        return new SonarAudioConfiguration(id, name, (Channel)ChannelExtensions.FromDictKey(vDevice));
    }

    public void SetConfig(string configId)
    {
        if (string.IsNullOrEmpty(configId)) throw new Exception("Couldn't retrieve config id");

        new HttpFetcher().Put("configs/" + configId + "/select");
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