using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.IO.Pipelines; 
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MeansToAnEndServer;

public class Worker(ILogger<Worker> logger) : BackgroundService
{
    private TcpListener? _tcpListener;
    private readonly int _port = 8080;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _tcpListener = new TcpListener(IPAddress.Any, _port);
        _tcpListener.Start();
        
        logger.LogInformation("Means to an End server started on port {Port}", _port);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var tcpClient = await _tcpListener.AcceptTcpClientAsync(stoppingToken);
                _ = HandleClientAsync(tcpClient, stoppingToken);
            }
            catch (ObjectDisposedException)
            {
                break;
            }
        }
    }

    private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
    {
        var prices = new SortedDictionary<int, int>();
        
        using (client)
        await using (var stream = client.GetStream())
        {
            var pipe = new Pipe();
            var writing = FillPipeAsync(stream, pipe.Writer, cancellationToken);
            var reading = ReadPipeAsync(client, pipe.Reader, prices, stream, cancellationToken);

            await Task.WhenAny(writing, reading);
        }
    }

    private async Task FillPipeAsync(NetworkStream stream, PipeWriter writer, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            Memory<byte> memory = writer.GetMemory(512);
            int bytesRead = await stream.ReadAsync(memory, cancellationToken);
            if (bytesRead == 0) break;
            
            writer.Advance(bytesRead);
            var result = await writer.FlushAsync(cancellationToken);
            if (result.IsCompleted) break;
        }
        await writer.CompleteAsync();
    }

    private async Task ReadPipeAsync(
        TcpClient client,
        PipeReader reader,
        SortedDictionary<int, int> prices,
        NetworkStream stream,
        CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && client.Connected)
        {
            ReadResult result = await reader.ReadAsync(cancellationToken);
            ReadOnlySequence<byte> buffer = result.Buffer;

            while (buffer.Length >= 9)
            {
                var message = buffer.Slice(0, 9);
                var messageType = message.First.Span[0];

                byte[] msgBytes = message.ToArray();

                switch (messageType)
                {
                    case (byte)'I':
                        Insert(client, msgBytes, prices);
                        break;
                    case (byte)'Q':
                        await Query(client, msgBytes, prices, stream, cancellationToken);
                        break;
                }

                buffer = buffer.Slice(9);
            }

            reader.AdvanceTo(buffer.Start, buffer.End);

            if (result.IsCompleted) break;
        }
        await reader.CompleteAsync();
    }

    private async Task Query(TcpClient client,  byte[] buffer, SortedDictionary<int, int> prices,
        NetworkStream stream, CancellationToken ct)
    {
        var minTime = BitConverter.ToInt32(buffer.AsSpan(1, 4).ToArray().Reverse().ToArray(), 0);
        var maxTime = BitConverter.ToInt32(buffer.AsSpan(5, 4).ToArray().Reverse().ToArray(), 0);
                            
        logger.LogInformation("Querying average from {MinTime} to {MaxTime} for {Client}", minTime, maxTime, client.Client.RemoteEndPoint);
                            
        var average = CalculateAverage(prices, minTime, maxTime);
        
        var result = BitConverter.GetBytes(average).Reverse().ToArray();
                            
        await stream.WriteAsync(result, 0, 4, ct);
        await stream.FlushAsync(ct);
        logger.LogInformation("Sent average {Average} to {Client}", average, client.Client.RemoteEndPoint);
    }

    private void Insert(TcpClient client, byte[] buffer, SortedDictionary<int, int> prices)
    {
        var timestamp = BitConverter.ToInt32(buffer.AsSpan(1, 4).ToArray().Reverse().ToArray(), 0);
        var price = BitConverter.ToInt32(buffer.AsSpan(5, 4).ToArray().Reverse().ToArray(), 0);
                            
        prices[timestamp] = price;
        logger.LogInformation("Received {Price} from {Client}", price, client.Client.RemoteEndPoint);
    }

    private static int CalculateAverage(SortedDictionary<int, int> prices, int minTime, int maxTime)
    {
        if (minTime > maxTime) return 0;
        
        var validPrices = prices
            .Where(p => p.Key >= minTime && p.Key <= maxTime)
            .Select(p => (long)p.Value)
            .ToList();
            
        if (validPrices.Count == 0) return 0;
        
        long sum = validPrices.Sum();
        long average = sum / validPrices.Count;
    
        return (int)average;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _tcpListener?.Stop();
        await base.StopAsync(cancellationToken);
    }
}
