using System.Net.Sockets;

namespace ProtoHackers.Problem0;

public class EchoService : ITcpService
{
    public async Task HandleClient(Socket conn)
    {
        Console.WriteLine($"Reading data from {conn.RemoteEndPoint}");

        var buffer = new byte[conn.ReceiveBufferSize];

        while (true)
        {
            int received = await conn.ReceiveAsync(buffer, SocketFlags.None);
            if (received == 0)
            {
                Console.WriteLine($"Connection closed to {conn.RemoteEndPoint}");
                conn.Close();
                break;
            }

            await conn.SendAsync(buffer[..received], SocketFlags.None);
        }
    }
}