using System.Net;
using System.Net.Sockets;

namespace Protohackers;

public static class TcpServer
{
    private const int Port = 8001;

    public static Socket Start()
    {
        var listener = new Socket(SocketType.Stream, ProtocolType.Tcp);
        listener.Bind(new IPEndPoint(IPAddress.Any, Port));
        listener.Listen();
        
        Console.WriteLine($"Start listen on {listener.LocalEndPoint} ...");
        return listener;
    }
}