using SteelSeriesAPI.Sonar.Exceptions;
using SteelSeriesAPI.Sonar.Enums;
using SteelSeriesAPI.Sonar.Http;
using SteelSeriesAPI.Sonar.Models;
using SteelSeriesAPI.Sonar.Interfaces.Managers;

using System.Text.Json;

namespace SteelSeriesAPI.Sonar.Managers;

public class RoutedProcessManager : IRoutedProcessManager
{
    public IEnumerable<RoutedProcess> GetRoutedProcesses(Channel channel)
    {
        if (channel == Channel.MASTER)
        {
            throw new MasterChannelNotSupportedException();
        }
        
        JsonDocument audioDeviceRoutings = new Fetcher().Provide("AudioDeviceRouting");

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
    
    public void RouteProcessToChannel(int pId, Channel channel)
    {
        if (channel == Channel.MASTER)
        {
            throw new MasterChannelNotSupportedException();
        }
        
        JsonDocument audioDeviceRouting = new Fetcher().Provide("AudioDeviceRouting");

        foreach (var element in audioDeviceRouting.RootElement.EnumerateArray())
        {
            if (element.GetProperty("role").GetString() == channel.ToDictKey())
            {
                if (channel == Channel.MIC)
                {
                    new Fetcher().Put("AudioDeviceRouting/capture/" + element.GetProperty("deviceId").GetString() + "/" + pId);
                    break;
                }
                else
                {
                    new Fetcher().Put("AudioDeviceRouting/render/" + element.GetProperty("deviceId").GetString() + "/" + pId);
                    break;
                }
            }
        }
    }
}