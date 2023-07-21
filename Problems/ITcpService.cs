using System.Net.Sockets;

namespace ProtoHackers;

public interface ITcpService
{
    public Task Handle(Socket socket);
}