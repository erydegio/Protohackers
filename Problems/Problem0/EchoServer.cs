using System.Net.Sockets;

namespace Protohackers.Problem0;

public class EchoServer : ITcpServer
{
    public async Task Handle(Socket socket)
    {
        Console.WriteLine("Reading data from" + socket.RemoteEndPoint);
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