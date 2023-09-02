using System.Net;
using System.Net.Sockets;

namespace ProtoHackers;

public static class TcpListener
{
    public static Socket Start(int port)
    {
        var listener = new Socket(SocketType.Stream, ProtocolType.Tcp);
        listener.Bind(new IPEndPoint(IPAddress.Any, port));
        listener.Listen();
        
        Console.WriteLine($"Start listening on port {port}");
        return listener;
    }
}