using System.Net;
using System.Net.Sockets;
using System.Text;
using ProtoHackers;
using Protohackers.Problem01;

namespace Tests.Problem01;

public class Problem01Tests
{
    private const int ServerPort = 9002;
    private TcpServer<PrimeService> _sut;

    [SetUp]
    public void Setup()
    {
        _sut = new TcpServer<PrimeService>(ServerPort);
        _sut.Listen();
    }

    [TearDown]
    public void TearDown()
    {
        _sut.Close();
    }

    [TestCase("{\"method\":\"isPrime\",\"number\":97}\n", "true")]
    [TestCase("{\"method\":\"isPrime\",\"number\":9}\n", "false")]
    [TestCase("{\"method\":\"isPrime\",\"number\":85685913980283186891777091683798721837984232898648870528,\"bignumber\":true}\n", "false")]
    [TestCase("{\"method\":\"isPrime\",\"ignore\":{\"method\":\"isPrime\",\"number\":\"624413\"},\"number\":1548528}\n", "false")]
    public async Task ServerReceiveConformedRequest(string message, string isPrime)
    {
        byte[] buffer = new byte[256];
        using var client = new TcpClient();
        var expected = "{\"method\":\"isPrime\",\"prime\":" + isPrime + "}\n";
        await client.ConnectAsync(IPAddress.Loopback, ServerPort);
        await client.GetStream().WriteAsync(Encoding.UTF8.GetBytes(message));
        int bytesRead = await client.GetStream().ReadAsync(buffer);

        string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

        client.Close();
        Assert.That(response, Is.EqualTo(expected));
    }

    [TestCase("hola\n")]
    [TestCase("{\"method\":\"wrongMethod\",\"number\":21}\n")]
    [TestCase("{\"number\":5669120}\n")]
    public async Task ServerReceiveMalformedRequest(string message)
    {
        var expected = "{\"method\":\"malformed\",\"prime\":false}\n";
        byte[] buffer = new byte[256];
        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, ServerPort);
        await client.GetStream().WriteAsync(Encoding.UTF8.GetBytes(message));
        int bytesRead = await client.GetStream().ReadAsync(buffer);

        string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

        client.Close();
        Assert.That(response, Is.EqualTo(expected));
    }
}