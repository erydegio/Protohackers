using System.Net.Sockets;

namespace MeansToAnEndServer;

public class MeansToAnEndClient(string host = "localhost", int port = 8080) : IDisposable
{
    private TcpClient? _tcpClient;
    private NetworkStream? _stream;

    public async Task ConnectAsync()
    {
        _tcpClient = new TcpClient();
        await _tcpClient.ConnectAsync(host, port);
        _stream = _tcpClient.GetStream();
        Console.WriteLine($"Connected to {host}:{port}");
    }

    public async Task InsertPriceAsync(int timestamp, int price)
    {
        if (_stream == null)
            throw new InvalidOperationException("Client not connected");

        var message = new byte[9];
        message[0] = (byte)'I';
        
        // Convertire timestamp in big-endian
        var timestampBytes = BitConverter.GetBytes(timestamp);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(timestampBytes);
        timestampBytes.CopyTo(message, 1);
        
        // Convertire price in big-endian
        var priceBytes = BitConverter.GetBytes(price);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(priceBytes);
        priceBytes.CopyTo(message, 5);

        await _stream.WriteAsync(message);
        Console.WriteLine($"Inserted: timestamp={timestamp}, price={price}");
    }

    public async Task<int> QueryAverageAsync(int minTime, int maxTime)
    {
        if (_stream == null)
            throw new InvalidOperationException("Client not connected");

        var message = new byte[9];
        message[0] = (byte)'Q';
        
        // Convertire minTime in big-endian
        var minTimeBytes = BitConverter.GetBytes(minTime);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(minTimeBytes);
        minTimeBytes.CopyTo(message, 1);
        
        // Convertire maxTime in big-endian
        var maxTimeBytes = BitConverter.GetBytes(maxTime);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(maxTimeBytes);
        maxTimeBytes.CopyTo(message, 5);

        await _stream.WriteAsync(message);

        // Leggere la risposta (4 byte)
        var response = new byte[4];
        int bytesRead = 0;
        while (bytesRead < 4)
        {
            int read = await _stream.ReadAsync(response, bytesRead, 4 - bytesRead);
            if (read == 0)
                throw new InvalidOperationException("Connection closed by server");
            bytesRead += read;
        }

        // Convertire da big-endian
        if (BitConverter.IsLittleEndian)
            Array.Reverse(response);
        
        int average = BitConverter.ToInt32(response, 0);
        Console.WriteLine($"Query: minTime={minTime}, maxTime={maxTime}, average={average}");
        return average;
    }

    public void Dispose()
    {
        _stream?.Dispose();
        _tcpClient?.Dispose();
        Console.WriteLine("Client disconnected");
    }
}
