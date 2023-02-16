using System.Net;
using System.Net.Sockets;

namespace ProtoHackers;

public class TcpEchoServer
{
    private readonly string _ip;
    private readonly int _port;
    
    public TcpEchoServer(string ip, int port)
    {
        _ip = ip;
        _port = port;
    }
    
    public async Task Run()
    {
        var listener = new TcpListener(IPAddress.Parse(_ip), _port);
        listener.Start();
        Console.WriteLine($"Start listen on {listener.LocalEndpoint} ...");

        while (true)
        {
            var client = await listener.AcceptTcpClientAsync();
            Console.WriteLine("Connected to client ..." + client.Client.RemoteEndPoint);
            Task.Run(() => EchoData(client));
        }
    }

    private async Task EchoData(TcpClient client)
    {
        Console.WriteLine("Reading data from" + client.Client.RemoteEndPoint);

        try
        {
            var buffer = new byte[1_024];
            await using var stream = client.GetStream();
            
            while (client.Connected)
            {
                var data = await stream.ReadAsync(buffer);
                if (data == 0) { break; }
                
                await stream.WriteAsync(buffer.AsMemory(0, data));
            }
            
            Console.WriteLine("closing socket " + client.Client.RemoteEndPoint + "....");
            client.Close();
            client.Dispose();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }
}