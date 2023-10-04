using System.Net.Sockets;

namespace ProtoHackers;

public interface ITcpService
{
    public Task HandleClient(Socket socket);
}