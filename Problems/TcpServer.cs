using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace ProtoHackers;

public class TcpServer<TService> where TService : ITcpService, new()
{
    private Socket _listener;

    public TcpServer() =>  _listener = Init();
    
        private Socket Init(int port = 9001)
    {
        _listener = new Socket(SocketType.Stream, ProtocolType.Tcp);
        _listener.Bind(new IPEndPoint(IPAddress.Any, port));
        _listener.Listen();
        Console.WriteLine("Start listen...");
        
        return _listener;
    }

    public async Task Listen()
    {
        while (true)
        {
            var conn = await _listener.AcceptAsync();
            _ = new TService().HandleClient(conn); 
        }
    }
}