using System.Collections;
using System.Text.Json;
using SteelSeriesAPI.Sonar.Interfaces;
using SteelSeriesAPI.Sonar.Enums;
using SteelSeriesAPI.Sonar.Models;

namespace SteelSeriesAPI.Sonar.Http;

public class SonarHttpProvider : ISonarDataProvider
{
    public Mode GetMode()
    {
        string mode = new HttpFetcher().Provide("mode").RootElement.ToString();
        
        return (Mode)ModeExtensions.FromDictKey(mode, ModeMapChoice.StreamDict);
    }

    #region ChatMix

    public double GetChatMixBalance()
    {
        JsonDocument chatMix = new HttpFetcher().Provide("chatMix");

        return chatMix.RootElement.GetProperty("balance").GetDouble();
    }

    public bool GetChatMixState()
    {
        JsonDocument chatMix = new HttpFetcher().Provide("chatMix");
        string cState = chatMix.RootElement.GetProperty("state").ToString();
        if (cState == "enabled")
        {
            return true;
        }
        else if (cState == "differentDeviceSelected")
        {
            return false;
        }

        return false;
    }
    
    #endregion

    public bool GetRedirectionState(Channel channel, Mix mix)
    {
        if (channel == Channel.MASTER)
        {
            throw new Exception("Can't get redirection state for master");
        }

        JsonDocument streamRedirections = new HttpFetcher().Provide("streamRedirections");
        JsonElement streamChannel = default;

        foreach (var element in streamRedirections.RootElement.EnumerateArray())
        {
            if (element.GetProperty("streamRedirectionId").GetString() == mix.ToDictKey())
            {
                streamChannel = element;
                break;
            }
        }

        JsonElement status = default;

        foreach (var element in streamChannel.GetProperty("status").EnumerateArray())
        {
            if (element.GetProperty("role").GetString() == channel.ToDictKey())
            {
                status = element;
                break;
            }
        }

        bool state = status.GetProperty("isEnabled").GetBoolean();

        return state;
    }

    public bool GetAudienceMonitoringState()
    {
        JsonDocument streamMonitoring = new HttpFetcher().Provide("streamRedirections/isStreamMonitoringEnabled");

        return streamMonitoring.RootElement.GetBoolean();
    }

    public IEnumerable<RoutedProcess> GetRoutedProcess(Channel channel)
    {
        if (channel == Channel.MASTER)
        {
            throw new Exception("Can't get routed process for Master");
        }
        
        JsonDocument audioDeviceRoutings = new HttpFetcher().Provide("AudioDeviceRouting");

        foreach (var element in audioDeviceRoutings.RootElement.EnumerateArray())
        {
            if (element.GetProperty("role").GetString() != channel.ToDictKey())
            {
                continue;
            }

            var audioSessions = element.GetProperty("audioSessions");

            foreach (var session in audioSessions.EnumerateArray())
            {
                string id = session.GetProperty("id").GetString().Split("|")[0];
                string processName = session.GetProperty("processName").GetString();
                int pId = session.GetProperty("processId").GetInt32();
                RoutedProcessState state = (RoutedProcessState)RoutedProcessStateExtensions.FromDictKey(session.GetProperty("state").GetString());
                string displayName = session.GetProperty("displayName").GetString();

                if (processName == "Idle" && displayName == "Idle" && state == RoutedProcessState.INACTIVE && pId == 0)
                {
                    continue;
                }
                
                yield return new RoutedProcess(id, processName, pId, state, displayName);
            }
        }
    }
}