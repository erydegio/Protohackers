namespace ProtoHackers.Problem01;

public class PrimeTime
{
    public static async Task Init()
    {
        using var server = TcpListener.Start(9001);
        while (true)
        {
            var socket = await server.AcceptAsync();
            Console.WriteLine("Connected to ..." + socket.RemoteEndPoint); 
            
            _ = new PrimeService().Handle(socket);
        }
    }
}