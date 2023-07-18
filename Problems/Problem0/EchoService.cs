using System.Net.Sockets;

namespace ProtoHackers.Problem0;

public class EchoService : ITcpService
{
    private const uint BufferSize = 4096;
    public async Task Handle(Socket conn)
    {
        Console.WriteLine($"Reading data from {conn.RemoteEndPoint}");
        
        var buffer = new byte[BufferSize];
        try
        {
            while (true)
            {
                int received = await conn.ReceiveAsync(buffer, SocketFlags.None);
                if (received == 0)
                    break;
                
                await conn.SendAsync(buffer[..received], SocketFlags.None);
            }
        }
        finally
        {
            conn.Dispose();
        }
    }
}