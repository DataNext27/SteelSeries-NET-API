using System.Net;
using System.Net.Sockets;
using System.Text;
using SteelSeriesAPI.Interfaces;

namespace SteelSeriesAPI.Sonar;

public class SonarSocket : ISonarSocket
{
    public bool IsConnected => _socket?.IsBound ?? false;
    
    private readonly Thread _listenerThread;
    private readonly Uri _sonarWebServerAddress;
    private readonly SonarEventManager _sonarEventManager;
    private Socket _socket;

    private bool _isClosing;

    public SonarSocket(string sonarWebServerAddress, SonarEventManager sonarEventManager)
    {
        _sonarWebServerAddress = new Uri(sonarWebServerAddress);
        _listenerThread = new Thread(ListenerThreadSync) { IsBackground = false };
        _sonarEventManager = sonarEventManager;
    }

    public bool Connect()
    {
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.IP);
        try
        {
            var ip = _sonarWebServerAddress.Host;
            var port = _sonarWebServerAddress.Port;
            _socket.Bind(new IPEndPoint(IPAddress.Parse(ip), port));
            
            return _socket.IsBound;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    public bool Listen()
    {
        if (!IsConnected)
        {
            throw new Exception("Listener need to be connected before listening");
        }
        
        _listenerThread.Start();

        return _listenerThread.IsAlive;
    }

    public void CloseSocket()
    {
        _isClosing = true;
        
        _socket.Shutdown(SocketShutdown.Both);
        _socket.Close();
    }
    
    private void ListenerThreadSync()
    {
        _socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.HeaderIncluded, true);
        byte[] optionIn = new byte[4] { 1, 0, 0, 0 };
        byte[] optionOut = new byte[4];
        _socket.IOControl(IOControlCode.ReceiveAll, optionIn, optionOut);
        
        byte[] buffer = new byte[4096];
        EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Parse(_sonarWebServerAddress.Host), _sonarWebServerAddress.Port);
        
        try
        {
            while (_socket.IsBound && !_isClosing)
            {
                int bytesRead = 0;
                try
                {
                    bytesRead = _socket.ReceiveFrom(buffer, ref remoteEndPoint);
                }
                catch (SocketException e)
                {
                    continue;
                }
                
                string data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                if (data.Contains("PUT "))
                {
                    string putData = "";
                    List<string> httpData = new List<string>(data.Split("\n"));
                    foreach (string line in httpData)
                    {
                        if (line.Contains("PUT "))
                        {
                            putData = line;
                            break;
                        }
                    }
                    if (!string.IsNullOrEmpty(putData))
                    {
                        string path = putData.Split("PUT ")[1].Split(" HTTP")[0];
                        // Console.WriteLine(path); // For debugging
                        _sonarEventManager.HandleEvent(path);  // Invoke events
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}