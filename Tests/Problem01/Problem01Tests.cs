using System.Net;
using System.Net.Sockets;
using System.Text;
using static ProtoHackers.Problem01.PrimeTime;

namespace Tests.Problem01;

public class Problem01Tests
{
    private const int ServerPort = 9001;
    
    [SetUp]
    public async Task Setup()
    {
        Init();
    }

    [TestCase("{\"method\":\"isPrime\",\"number\":97}\n", "true")]
    [TestCase("{\"method\":\"isPrime\",\"number\":21}\n", "false")]
    public async Task ServerReceiveConformedRequest(string message, string isPrime)
    {
        byte[] buffer = new byte[256];
        using var client = new TcpClient();
        var expected = "{\"method\":\"isPrime\",\"prime\":"+isPrime+"}";

        await client.ConnectAsync(IPAddress.Loopback, ServerPort);
        await client.GetStream().WriteAsync(Encoding.UTF8.GetBytes(message));
        int bytesRead = await client.GetStream().ReadAsync(buffer);
        string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

        client.Close();

        Assert.AreEqual(response, expected);
    }
    
    [TestCase("hola\n")]
    [TestCase("{\"method\":\"wrongMethod\",\"number\":21}\n")]
    public async Task ServerReceiveMalformedRequest(string message)
    {
        byte[] buffer = new byte[256];
        using var client = new TcpClient();
        var expected = "malformed";

        await client.ConnectAsync(IPAddress.Loopback, ServerPort);
        await client.GetStream().WriteAsync(Encoding.UTF8.GetBytes(message));
        int bytesRead = await client.GetStream().ReadAsync(buffer);
        string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

        client.Close();

        Assert.AreEqual(response, expected);
    }
}