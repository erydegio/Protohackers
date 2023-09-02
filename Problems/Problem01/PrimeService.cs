using System.Buffers;
using System.IO.Pipelines;
using System.Net.Sockets;

namespace ProtoHackers.Problem01;

public class PrimeService : ITcpService
{
    public async Task Handle(Socket conn) 
    {
        Console.WriteLine($"Reading data from {conn.RemoteEndPoint}");
        
        var stream = new NetworkStream(conn);
        var reader = PipeReader.Create(stream);
        
        while (true)
        {
            ReadResult result = await reader.ReadAsync();
            ReadOnlySequence<byte> buffer = result.Buffer;

            while (TryReadLine(ref buffer, out ReadOnlySequence<byte> line))
            {
                ProcessLine(line);
            }

            // Tell the PipeReader how much of the buffer has been consumed.
            reader.AdvanceTo(buffer.Start, buffer.End);

            // Stop reading if there's no more data coming
            if (result.IsCompleted)
            {
                break;
            }
        }
    }

    private void ProcessLine(in ReadOnlySequence<byte> readOnlySequence)
    {
        // Line parsing logic
    }

    private bool TryReadLine(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> line)
    {
        // Look for EOL.
        SequencePosition? position = buffer.PositionOf((byte)'\n');

        if (position == null)
        {
            line = default;
            return false;
        }

        // Skip the line + the \n.
        line = buffer.Slice(0, position.Value);
        buffer = buffer.Slice(buffer.GetPosition(1, position.Value));
        return true;
    }

    public bool IsPrime(decimal n, int i=2)
    {
        if (!IsInteger(n))
            return false;
        if (n <= 2) 
            return n == 2; 
        if (n % i == 0) 
            return false; 
        return i * i > n || IsPrime(n, i + 1);
        
        bool IsInteger(decimal input)
        {
                // Check if the decimal number has no fractional part
                return input == Math.Floor(input);
        }
    }

    private Request ReadRequest()
    {
        return new Request();
    }

    private Response HandleRequest(Request req)
    {
        return new Response("", false);
    }

}

public record Response(string Method, bool IsPrime);


public record Request();


