namespace ProtoHackers.Problem0;

public static class SmokeTest
{
    public static async Task Init()
    {
        using var server = TcpListener.Start();
        while (true)
        {
            var socket = await server.AcceptAsync();
            Console.WriteLine("Connected to ..." + socket.RemoteEndPoint); 
            
            _ = new EchoService().Handle(socket);
        }
    }
}