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
    [TestCase("{\"method\":\"isPrime\",\"number\":97.8}\n", "false")]
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

        Assert.AreEqual(expected, response);
    }
    
    [TestCase("hola\n", "{\"malformed\":\"Invalid json format.\"}\n")]
    [TestCase("{\"method\":\"wrongMethod\",\"number\":21}\n", "{\"malformed\":\"Incorrect method.\"}\n")]
    public async Task ServerReceiveMalformedRequest(string message, string expected)
    {
        byte[] buffer = new byte[256];
        using var client = new TcpClient();

        await client.ConnectAsync(IPAddress.Loopback, ServerPort);
        await client.GetStream().WriteAsync(Encoding.UTF8.GetBytes(message));
        int bytesRead = await client.GetStream().ReadAsync(buffer);
        string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

        client.Close();

        Assert.AreEqual(expected, response);
    }
    
    [Test]
    public async Task ServerCanHandleMultipleRequest()
    {
        byte[] buffer = new byte[256];
        using var client = new TcpClient();
        var message1 = "{\"method\":\"isPrime\",\"number\":97}\n";
        var message2 = "{\"method\":\"isPrime\",\"number\":9}\n";
        var expected1 = "{\"method\":\"isPrime\",\"prime\":true}";
        var expected2 = "{\"method\":\"isPrime\",\"prime\":false}";
        await client.ConnectAsync(IPAddress.Loopback, ServerPort);
        
        await client.GetStream().WriteAsync(Encoding.UTF8.GetBytes(message1));
        int bytesRead1 = await client.GetStream().ReadAsync(buffer);
        string response1 = Encoding.UTF8.GetString(buffer, 0, bytesRead1).Trim();
        
        await client.GetStream().WriteAsync(Encoding.UTF8.GetBytes(message2));
        int bytesRead2 = await client.GetStream().ReadAsync(buffer);
        string response2 = Encoding.UTF8.GetString(buffer, 0, bytesRead2).Trim();

        client.Close();

        Assert.AreEqual(expected1, response1);
        Assert.AreEqual(expected2, response2);
    }
}