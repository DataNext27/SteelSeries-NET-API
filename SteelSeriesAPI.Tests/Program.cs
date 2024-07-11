using SteelSeriesAPI.EventArgs;
using SteelSeriesAPI.Sonar;
using SteelSeriesAPI.Sonar.Enums;

namespace SteelSeriesAPI.Tests;

class Program
{
    static void Main(string[] args)
    {
        SonarBridge sonarManager = new SonarBridge();
        sonarManager.WaitUntilSonarStarted();
        Console.WriteLine(new SonarRetriever().WebServerAddress());

        // sonarManager.StartListener();
        
        // do
        // {
        //     continue;
        // } while (Console.ReadKey(true).Key != ConsoleKey.Enter);
        // sonarManager.StopListener();
        // Console.WriteLine("End");
        var lastVol = sonarManager.GetVolume(Device.Media);
        Console.WriteLine(lastVol);
        sonarManager.SetVolume(0, Device.Media);
        sonarManager.SetMute(true, Device.Media);
        Console.WriteLine(sonarManager.GetMode());
        Console.WriteLine("" + sonarManager.GetVolume(Device.Master) + "  " + sonarManager.GetMute(Device.Master));
        Console.WriteLine("" + sonarManager.GetVolume(Device.Game) + "  " + sonarManager.GetMute(Device.Game));
        Console.WriteLine("" + sonarManager.GetVolume(Device.Chat) + "  " + sonarManager.GetMute(Device.Chat));
        Console.WriteLine("" + sonarManager.GetVolume(Device.Media) + "  " + sonarManager.GetMute(Device.Media));
        Console.WriteLine("" + sonarManager.GetVolume(Device.Aux) + "  " + sonarManager.GetMute(Device.Aux));
        Console.WriteLine("" + sonarManager.GetVolume(Device.Mic) + "  " + sonarManager.GetMute(Device.Mic));
        Console.WriteLine("-----------------Streamer-Monitoring-------------");
        Console.WriteLine("" + sonarManager.GetVolume(Device.Master, Mode.Streamer) + "  " + sonarManager.GetMute(Device.Master, Mode.Streamer));
        Console.WriteLine("" + sonarManager.GetVolume(Device.Game, Mode.Streamer) + "  " + sonarManager.GetMute(Device.Game, Mode.Streamer));
        Console.WriteLine("" + sonarManager.GetVolume(Device.Chat, Mode.Streamer) + "  " + sonarManager.GetMute(Device.Chat, Mode.Streamer));
        Console.WriteLine("" + sonarManager.GetVolume(Device.Media, Mode.Streamer) + "  " + sonarManager.GetMute(Device.Media, Mode.Streamer));
        Console.WriteLine("" + sonarManager.GetVolume(Device.Aux, Mode.Streamer) + "  " + sonarManager.GetMute(Device.Aux, Mode.Streamer));
        Console.WriteLine("" + sonarManager.GetVolume(Device.Mic, Mode.Streamer) + "  " + sonarManager.GetMute(Device.Mic, Mode.Streamer));
        Console.WriteLine("-----------------Streamer-Stream-------------");
        Console.WriteLine("" + sonarManager.GetVolume(Device.Master, Mode.Streamer, Channel.Stream) + "  " + sonarManager.GetMute(Device.Master, Mode.Streamer, Channel.Stream));
        Console.WriteLine("" + sonarManager.GetVolume(Device.Game, Mode.Streamer, Channel.Stream) + "  " + sonarManager.GetMute(Device.Game, Mode.Streamer, Channel.Stream));
        Console.WriteLine("" + sonarManager.GetVolume(Device.Chat, Mode.Streamer, Channel.Stream) + "  " + sonarManager.GetMute(Device.Chat, Mode.Streamer, Channel.Stream));
        Console.WriteLine("" + sonarManager.GetVolume(Device.Media, Mode.Streamer, Channel.Stream) + "  " + sonarManager.GetMute(Device.Media, Mode.Streamer, Channel.Stream));
        Console.WriteLine("" + sonarManager.GetVolume(Device.Aux, Mode.Streamer, Channel.Stream) + "  " + sonarManager.GetMute(Device.Aux, Mode.Streamer, Channel.Stream));
        Console.WriteLine("" + sonarManager.GetVolume(Device.Mic, Mode.Streamer, Channel.Stream) + "  " + sonarManager.GetMute(Device.Mic, Mode.Streamer, Channel.Stream));
        sonarManager.SetVolume(lastVol, Device.Media);
        sonarManager.SetMute(false, Device.Media);
        
        //sonarManager.SetMode(Mode.Classic);


        /*
        sonarManager.OnSteelSeriesSonarEvent += OnSteelSeriesEventHandler;

        GetVolume(); // Device, Mode, Channel       return double
        GetMute(); // Device, Mode, Channel         return bool
        GetAudioConfigurations(); // Device      return List<SonarAudioConfiguration>
        GetSelectedAudioConfiguration(); // Device      return SonarAudioConfiguration
        GetDeviceFromAudioConfiguration(); // configId      return Device
        GetChatMixBalance(); // return double -1<-0->1
        GetChatMixState(); // return bool
        GetRedircetionDevices(); // Direction     return  List<RedirectionDevice>
        GetSelectedRedircetionDevice(); // Device   return RedirectionDevice
        GetSelectedRedircetionDevice(); // Channel  return RedirectionDevice
        GetRedircetionDeviceFromId(); // deviceId   return RedirectionDevice
        GetRoutedProcesses(); // Device     return List<RoutedProcess>

        GetRedirectionState(); // Device, Channel   return bool
        GetAudienceMonitoringState(); // return bool


        SetVolume(); // double, Device, Mode, Channel
        SetMute(); // double, Device, Mode, Channel
        SetConfig(); // Device, SonarAudioConfiguration.Id
        SetChatMixBalance(); // double -1<-0->1
        SetRedirectionDevice(); // deviceId, Device
        SetRedirectionDevice(); // deviceId, Channel
        SetProcessToDeviceRouting(); // PID, Device

        SetRedirectionState();// Device Channel, bool
        SetAudienceMonitoringState(); // bool
        */
    }

    public static void OnSteelSeriesEventHandler(object sender, SonarEventArgs eventArgs)
    {
        
    }
}