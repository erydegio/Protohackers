using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Tests;

public class Problem0Tests
{
    private const int ServerPort = 9001;
    private const string Message = "test message";

    [SetUp]
    public void Setup()
    {
        Init();
    }

    [Test]
    public async Task ServerSendsSameDataBack()
    {
        byte[] buffer = new byte[100];
        using var client = new TcpClient();

        await client.ConnectAsync(IPAddress.Loopback, ServerPort);
        await client.GetStream().WriteAsync(Encoding.UTF8.GetBytes(Message));
        int bytesRead = await client.GetStream().ReadAsync(buffer);
        string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

        client.Close();

        Assert.That(response, Is.EqualTo(Message));
    }

    [Test]
    public async Task HandleAtLeast5SimultaneousClients()
    {
        var clients = new List<TcpClient>();
        const int numClients = 5;

        for (int i = 0; i < numClients; i++)
        {
            byte[] buffer = new byte[100];
            var client = new TcpClient();
            
            await client.ConnectAsync(IPAddress.Loopback, ServerPort);
            clients.Add(client);

            var data = Encoding.UTF8.GetBytes($"{Message}t {i + 1}");
            await client.GetStream().WriteAsync(data);
            int bytesRead = await client.GetStream().ReadAsync(buffer);
            string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            
            Assert.That(response, Is.EqualTo(data));
        }

        Assert.That(clients.All(client => client.Connected));

        foreach (var client in clients)
            client.Close();
    }
}