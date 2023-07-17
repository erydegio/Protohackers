using System.Net.Sockets;

namespace ProtoHackers.Problem0;

public class EchoService : ITcpService
{
    public async Task Handle(Socket socket)
    {
        Console.WriteLine($"Reading data from {socket.RemoteEndPoint}");
        try
        {
            await using var stream = new NetworkStream(socket, true);
            await stream.CopyToAsync(stream);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }
}