using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace ProtoHackers;

public abstract class TcpServer
{
    private Socket _listener;
    private readonly ILogger _logger;

    protected TcpServer(  ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("ProtoHackers.TcpServer");
        _logger = logger;
        _listener = Start();
       
    }
    private Socket Start(int port = 9001)
    {
        _listener = new Socket(SocketType.Stream, ProtocolType.Tcp);
        _listener.Bind(new IPEndPoint(IPAddress.Any, port));
        _listener.Listen();
        
        _logger.LogInformation($"Start listening on port {port}");
        
        return _listener;
    }

    public async Task Init()
    {
        while (true)
        {
            var conn = await _listener.AcceptAsync();
            _logger.LogInformation("Connected to ..." + conn.RemoteEndPoint); 
            
            _ = HandleConnection(conn);
        }
    }

    protected abstract Task HandleConnection(Socket socket);
}