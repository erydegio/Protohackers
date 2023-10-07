using System.Net;
using System.Net.Sockets;

namespace ProtoHackers;

public class TcpServer<TService> where TService : ITcpService, new()
{
    private Socket _listener;

    public TcpServer(int port = 9001) =>  _listener = Init(port);
    
        private Socket Init(int port)
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

    public void Close()
    { 
        _listener.Close();
    }
}