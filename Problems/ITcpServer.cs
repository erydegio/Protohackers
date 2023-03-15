using System.Net.Sockets;

namespace Protohackers;

public interface ITcpServer
{
    public Task Handle(Socket socket);
}