namespace Protohackers.Problem0;

public static class SmokeTest
{
    public static async Task ProblemOResponse()
    {
        var server = TcpServer.Start();
        while (true)
        {
            var socket = await server.AcceptAsync();
            Console.WriteLine("Connected to ..." + socket.RemoteEndPoint); 
            Task.Run(() => new EchoServer().Handle(socket));
        }
    }
}