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
        
        Console.WriteLine(sonarManager.GetMode());
        // Console.WriteLine("" + sonarManager.GetVolume(Device.Master) + "  " + sonarManager.GetMute(Device.Master));
        // Console.WriteLine("" + sonarManager.GetVolume(Device.Game) + "  " + sonarManager.GetMute(Device.Game));
        // Console.WriteLine("" + sonarManager.GetVolume(Device.Chat) + "  " + sonarManager.GetMute(Device.Chat));
        // Console.WriteLine("" + sonarManager.GetVolume(Device.Media) + "  " + sonarManager.GetMute(Device.Media));
        // Console.WriteLine("" + sonarManager.GetVolume(Device.Aux) + "  " + sonarManager.GetMute(Device.Aux));
        // Console.WriteLine("" + sonarManager.GetVolume(Device.Mic) + "  " + sonarManager.GetMute(Device.Mic));
        // Console.WriteLine("-----------------Streamer-Monitoring-------------");
        // Console.WriteLine("" + sonarManager.GetVolume(Device.Master, Mode.Streamer) + "  " + sonarManager.GetMute(Device.Master, Mode.Streamer));
        // Console.WriteLine("" + sonarManager.GetVolume(Device.Game, Mode.Streamer) + "  " + sonarManager.GetMute(Device.Game, Mode.Streamer));
        // Console.WriteLine("" + sonarManager.GetVolume(Device.Chat, Mode.Streamer) + "  " + sonarManager.GetMute(Device.Chat, Mode.Streamer));
        // Console.WriteLine("" + sonarManager.GetVolume(Device.Media, Mode.Streamer) + "  " + sonarManager.GetMute(Device.Media, Mode.Streamer));
        // Console.WriteLine("" + sonarManager.GetVolume(Device.Aux, Mode.Streamer) + "  " + sonarManager.GetMute(Device.Aux, Mode.Streamer));
        // Console.WriteLine("" + sonarManager.GetVolume(Device.Mic, Mode.Streamer) + "  " + sonarManager.GetMute(Device.Mic, Mode.Streamer));
        // Console.WriteLine("-----------------Streamer-Stream-------------");
        // Console.WriteLine("" + sonarManager.GetVolume(Device.Master, Mode.Streamer, Channel.Stream) + "  " + sonarManager.GetMute(Device.Master, Mode.Streamer, Channel.Stream));
        // Console.WriteLine("" + sonarManager.GetVolume(Device.Game, Mode.Streamer, Channel.Stream) + "  " + sonarManager.GetMute(Device.Game, Mode.Streamer, Channel.Stream));
        // Console.WriteLine("" + sonarManager.GetVolume(Device.Chat, Mode.Streamer, Channel.Stream) + "  " + sonarManager.GetMute(Device.Chat, Mode.Streamer, Channel.Stream));
        // Console.WriteLine("" + sonarManager.GetVolume(Device.Media, Mode.Streamer, Channel.Stream) + "  " + sonarManager.GetMute(Device.Media, Mode.Streamer, Channel.Stream));
        // Console.WriteLine("" + sonarManager.GetVolume(Device.Aux, Mode.Streamer, Channel.Stream) + "  " + sonarManager.GetMute(Device.Aux, Mode.Streamer, Channel.Stream));
        // Console.WriteLine("" + sonarManager.GetVolume(Device.Mic, Mode.Streamer, Channel.Stream) + "  " + sonarManager.GetMute(Device.Mic, Mode.Streamer, Channel.Stream));
        // Console.WriteLine("----AudioConfigs----------");
        // Console.WriteLine(sonarManager.GetAllAudioConfigurations().ToList());
        // Console.WriteLine("----Mic-Configs----------");
        // Console.WriteLine(sonarManager.GetAudioConfigurations(Device.Mic).ToList());
        // Console.WriteLine("----Current Media Config----------");
        // Console.WriteLine(sonarManager.GetSelectedAudioConfiguration(Device.Media).Name);
        // Console.WriteLine("----Device from config ID----------");
        // Console.WriteLine(sonarManager.GetDeviceFromAudioConfigurationId("29ae2c02-792b-4487-863c-dc3e11a7a469"));
        // Console.WriteLine("--------ChatMix---------");
        // Console.WriteLine(sonarManager.GetChatMixBalance());
        // Console.WriteLine(sonarManager.GetChatMixState());



        // sonarManager.SetMode(Mode.Classic);
        // sonarManager.SetVolume(0.2, Device.Media);
        // sonarManager.SetMute(false, Device.Media);
        // sonarManager.SetConfig("e6400b4d-f682-4199-af90-8a778788a31c");
        // sonarManager.SetConfig(Device.Media, "Default");
        // sonarManager.SetChatMixBalance(0.5);


        /*
        sonarManager.OnSteelSeriesSonarEvent += OnSteelSeriesEventHandler;


        GetRedircetionDevices(); // Direction     return  List<RedirectionDevice>
        GetSelectedRedircetionDevice(); // Device   return RedirectionDevice
        GetSelectedRedircetionDevice(); // Channel  return RedirectionDevice
        GetRedircetionDeviceFromId(); // deviceId   return RedirectionDevice
        GetRoutedProcesses(); // Device     return List<RoutedProcess>

        GetRedirectionState(); // Device, Channel   return bool
        GetAudienceMonitoringState(); // return bool


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