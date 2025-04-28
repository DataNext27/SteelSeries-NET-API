using SteelSeriesAPI.Sonar.Exceptions;
using SteelSeriesAPI.Sonar.Enums;
using SteelSeriesAPI.Sonar.Http;
using SteelSeriesAPI.Sonar.Interfaces.Managers;
using SteelSeriesAPI.Sonar.Models;

using System.Text.Json;

namespace SteelSeriesAPI.Sonar.Managers;

internal class RoutedProcessManager : IRoutedProcessManager
{
    public IEnumerable<RoutedProcess> GetAllRoutedProcesses()
    {
        JsonElement audioDeviceRouting = new Fetcher().Provide("AudioDeviceRouting").RootElement;

        foreach (JsonElement device in audioDeviceRouting.EnumerateArray())
        {
            string role = device.GetProperty("role").GetString()!;
            if (role != "none")
            {
                foreach (JsonElement session in device.GetProperty("audioSessions").EnumerateArray())
                {
                    int processId = session.GetProperty("processId").GetInt32();
                    string processName = session.GetProperty("processName").GetString()!;
                    string displayName = session.GetProperty("displayName").GetString()!;
                    
                    if (processId == 0 && processName == "Idle" && displayName == "Idle") continue;
                    
                    RoutedProcessState state = (RoutedProcessState)RoutedProcessStateExtensions.FromDictKey(device.GetProperty("state").GetString()!)!;
                    Channel channel = (Channel)ChannelExtensions.FromDictKey(role)!;
                    string processPath = session.GetProperty("id").GetString()!.Split("|")[1].Replace('\\', '/');
                    
                    yield return new RoutedProcess(processId, processName, displayName, state, channel, processPath);
                }
            }
        }
    }

    public IEnumerable<RoutedProcess> GetAllActiveRoutedProcesses()
    {
        JsonElement audioDeviceRouting = new Fetcher().Provide("AudioDeviceRouting").RootElement;

        foreach (JsonElement device in audioDeviceRouting.EnumerateArray())
        {
            string role = device.GetProperty("role").GetString()!;
            if (role != "none")
            {
                foreach (JsonElement session in device.GetProperty("audioSessions").EnumerateArray())
                {
                    if (session.GetProperty("state").GetString() == "active")
                    {
                        int processId = session.GetProperty("processId").GetInt32();
                        string processName = session.GetProperty("processName").GetString()!;
                        string displayName = session.GetProperty("displayName").GetString()!;
                        
                        if (processId == 0 && processName == "Idle" && displayName == "Idle") continue;
                        
                        RoutedProcessState state = RoutedProcessState.ACTIVE;
                        Channel channel = (Channel)ChannelExtensions.FromDictKey(role)!;
                        string processPath = session.GetProperty("id").GetString()!.Split("|")[1].Replace('\\', '/');
                        
                        yield return new RoutedProcess(processId, processName, displayName, state, channel, processPath);
                    }
                }
            }
        }
    }

    public IEnumerable<RoutedProcess> GetRoutedProcesses(Channel channel)
    {
        if (channel == Channel.MASTER)
        {
            throw new MasterChannelNotSupportedException();
        }
        
        JsonElement audioDeviceRouting = new Fetcher().Provide("AudioDeviceRouting").RootElement;

        foreach (JsonElement device in audioDeviceRouting.EnumerateArray())
        {
            if (device.GetProperty("role").GetString() == channel.ToDictKey())
            {
                foreach (JsonElement session in device.GetProperty("audioSessions").EnumerateArray())
                {
                    int processId = session.GetProperty("processId").GetInt32();
                    string processName = session.GetProperty("processName").GetString()!;
                    string displayName = session.GetProperty("displayName").GetString()!;
                    
                    if (processId == 0 && processName == "Idle" && displayName == "Idle") continue;
                    
                    RoutedProcessState state = (RoutedProcessState)RoutedProcessStateExtensions.FromDictKey(device.GetProperty("state").GetString()!)!;
                    string processPath = session.GetProperty("id").GetString()!.Split("|")[1].Replace('\\', '/');
                    
                    yield return new RoutedProcess(processId, processName, displayName, state, channel, processPath);
                }
            }
        }
    }

    public IEnumerable<RoutedProcess> GetActiveRoutedProcesses(Channel channel)
    {
        if (channel == Channel.MASTER)
        {
            throw new MasterChannelNotSupportedException();
        }
        
        JsonElement audioDeviceRouting = new Fetcher().Provide("AudioDeviceRouting").RootElement;

        foreach (JsonElement device in audioDeviceRouting.EnumerateArray())
        {
            if (device.GetProperty("role").GetString() == channel.ToDictKey())
            {
                foreach (JsonElement session in device.GetProperty("audioSessions").EnumerateArray())
                {
                    if (session.GetProperty("state").GetString() == "active")
                    {
                        int processId = session.GetProperty("processId").GetInt32();
                        string processName = session.GetProperty("processName").GetString()!;
                        string displayName = session.GetProperty("displayName").GetString()!;
                        
                        if (processId == 0 && processName == "Idle" && displayName == "Idle") continue;
                        
                        RoutedProcessState state = RoutedProcessState.ACTIVE;
                        string processPath = session.GetProperty("id").GetString()!.Split("|")[1].Replace('\\', '/');
                        
                        yield return new RoutedProcess(processId, processName, displayName, state, channel, processPath);
                    }
                }
            }
        }
    }

    public IEnumerable<RoutedProcess> GetRoutedProcessesById(int processId)
    {
        JsonElement audioDeviceRouting = new Fetcher().Provide("AudioDeviceRouting").RootElement;

        foreach (JsonElement device in audioDeviceRouting.EnumerateArray())
        {
            string role = device.GetProperty("role").GetString()!;
            if (role != "none")
            {
                foreach (JsonElement session in device.GetProperty("audioSessions").EnumerateArray())
                {
                    if (session.GetProperty("processId").GetInt32() == processId)
                    {
                        string processName = session.GetProperty("processName").GetString()!;
                        string displayName = session.GetProperty("displayName").GetString()!;
                        RoutedProcessState state = (RoutedProcessState)RoutedProcessStateExtensions.FromDictKey(device.GetProperty("state").GetString()!)!;
                        Channel channel = (Channel)ChannelExtensions.FromDictKey(role)!;
                        string processPath = session.GetProperty("id").GetString()!.Split("|")[1].Replace('\\', '/');
                    
                        yield return new RoutedProcess(processId, processName, displayName, state, channel, processPath);
                    }
                }
            }
        }
    }

    public IEnumerable<RoutedProcess> GetActiveRoutedProcessesById(int processId)
    {
        JsonElement audioDeviceRouting = new Fetcher().Provide("AudioDeviceRouting").RootElement;
        
        foreach (JsonElement device in audioDeviceRouting.EnumerateArray())
        {
            string role = device.GetProperty("role").GetString()!;
            if (role != "none")
            {
                foreach (JsonElement session in device.GetProperty("audioSessions").EnumerateArray())
                {
                    if (session.GetProperty("state").GetString() == "active" && session.GetProperty("processId").GetInt32() == processId)
                    {
                        string processName = session.GetProperty("processName").GetString()!;
                        string displayName = session.GetProperty("displayName").GetString()!;
                        RoutedProcessState state = RoutedProcessState.ACTIVE;
                        Channel channel = (Channel)ChannelExtensions.FromDictKey(role)!;
                        string processPath = session.GetProperty("id").GetString()!.Split("|")[1].Replace('\\', '/');
                        
                        yield return new RoutedProcess(processId, processName, displayName, state, channel, processPath);
                    }
                }
            }
        }

        throw new RoutedProcessNotFoundException("No active processes with id " + processId + " found");
    }

    public void RouteProcessToChannel(int processId, Channel channel)
    {
        if (channel == Channel.MASTER)
        {
            throw new MasterChannelNotSupportedException();
        }
        
        JsonElement audioDeviceRouting = new Fetcher().Provide("AudioDeviceRouting").RootElement;

        foreach (JsonElement device in audioDeviceRouting.EnumerateArray())
        {
            if (device.GetProperty("role").GetString() == channel.ToDictKey())
            {
                if (channel == Channel.MIC)
                {
                    new Fetcher().Put("AudioDeviceRouting/capture/" + device.GetProperty("deviceId").GetString() + "/" + processId);
                    break;
                }
                
                new Fetcher().Put("AudioDeviceRouting/render/" + device.GetProperty("deviceId").GetString() + "/" + processId);
                break;
            }
        }
    }

    public void RouteProcessToChannel(RoutedProcess process, Channel channel)
    {
        RouteProcessToChannel(process.ProcessId, channel);

        if (process.Channel == channel)
        {
            process.State = RoutedProcessState.ACTIVE;
        }
        else
        {
            process.State = RoutedProcessState.INACTIVE;
        }
    }
}