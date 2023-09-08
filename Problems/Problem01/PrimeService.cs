using System.Buffers;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace ProtoHackers.Problem01;

public class PrimeService : ITcpService
{
    private IRequestHandler _requestHandler;

    public PrimeService()
    {
        _requestHandler = new PrimeServiceRequestHandler();
    }
    public async Task Handle(Socket socket)
    {
        Console.WriteLine($"[{socket.RemoteEndPoint}]: connected");

        var stream = new NetworkStream(socket, true);
        var reader = PipeReader.Create(stream);
        // var writer = new StreamWriter(stream); //todo use pipewriter

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

        //Console.WriteLine($"[{socket.RemoteEndPoint}]: disconnected");
    }

    private async Task ProcessLine(ReadOnlySequence<byte> readOnlySequence, NetworkStream stream)
    {
        foreach (var segment in readOnlySequence)
        {
                try
                {
                    var response = _requestHandler.HandleRequest(segment);
                    var json = JsonSerializer.Serialize(response,
                        new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                    
                    Byte[] data = Encoding.ASCII.GetBytes(json);
                    await stream.WriteAsync(data);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    await stream.WriteAsync("malformed"u8.ToArray());
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

    public record PrimServiceResponse(string Method, bool Prime);
    public record PrimServiceRequest(string Method, decimal Number);
}