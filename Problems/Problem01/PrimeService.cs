using System.Buffers;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Exception = System.Exception;

namespace ProtoHackers.Problem01;

public class PrimeService : ITcpService
{
    private readonly IRequestHandler _requestHandler;

    public PrimeService()
    {
        _requestHandler = new PrimeServiceRequestHandler();
    }
    public async Task Handle(Socket socket)
    {
        Console.WriteLine($"[{socket.RemoteEndPoint}]: connected");

        var stream = new NetworkStream(socket, true);
        var reader = PipeReader.Create(stream);

        while (true)
        {
            ReadResult result = await reader.ReadAsync();
            ReadOnlySequence<byte> buffer = result.Buffer;

            while (TryReadLine(ref buffer, out ReadOnlySequence<byte> line))
            {
                await ProcessLine(line, stream);
            }

            reader.AdvanceTo(buffer.Start, buffer.End);

            // Stop reading if there's no more data coming.
            if (result.IsCompleted)
            {
                break;
            }
        }

        await reader.CompleteAsync();
    }

    private async Task ProcessLine(ReadOnlySequence<byte> readOnlySequence, NetworkStream stream)
    {
        foreach (var segment in readOnlySequence)
        {
            try
            {
                var response = _requestHandler.HandleRequest(segment);
                var json = JsonSerializer.Serialize(response,
                    new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase});
                var delimitedResponse = $"{json}\n";

                Byte[] data = Encoding.ASCII.GetBytes(delimitedResponse);
                await stream.WriteAsync(data);
            }
            catch (Exception e) when (e is JsonFormatException or MalformedRequestException)
            {
                var response = Encoding.ASCII.GetBytes("{\"malformed\":\""+$"{e.Message}"+"\"}\n");
                await stream.WriteAsync(response);
            }
        }
    }

    // Buffer the incoming data until a new line is found.
    private bool TryReadLine(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> line)
    {
        SequencePosition? position = buffer.PositionOf((byte)'\n');

        if (position == null)
        {
            line = default;
            return false;
        }

        // Skip the line + the \n.
        line = buffer.Slice(0, position.Value);

        // Remove the parsed message from the input buffer.
        buffer = buffer.Slice(buffer.GetPosition(1, position.Value));
        return true;
    }
}