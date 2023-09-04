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
        var writer = PipeWriter.Create(stream); // consumes data from the network and puts it in buffers.
        var reader = PipeReader.Create(stream); // constructing the appropriate data structures.

         Task write = WriteFromSocketAsync(conn, writer);
         Task read = ReadFromPipeAsync(reader);
        
        await Task.WhenAll(read, write);
    }

    private async Task WriteFromSocketAsync(Socket socket, PipeWriter writer)
    {
        const int minimumBufferSize = 512; //At least

        while (true)
        {
            Memory<byte> memory = writer.GetMemory(minimumBufferSize);
            try 
            {
                int bytesRead = await socket.ReceiveAsync(memory, SocketFlags.None);
                if (bytesRead == 0) break;
                
                // Tell the PipeWriter how much was read from the Socket
                writer.Advance(bytesRead);
            }
            catch (Exception ex)
            {
               // LogError(ex); todo
                break;
            }

            // Make the data available to the PipeReader
            FlushResult result = await writer.FlushAsync();

            if (result.IsCompleted) break;
        }

        // Tell the PipeReader that there's no more data coming
        await writer.CompleteAsync();
    }
    private async Task ReadFromPipeAsync(PipeReader reader)
    {
        while (true)
        {
            ReadResult result = await reader.ReadAsync();
            ReadOnlySequence<byte> buffer = result.Buffer;

            while (TryReadLine(ref buffer, out ReadOnlySequence<byte> line))
            {
                var response = ProcessLine(line);;
            }

            // Tell the PipeReader how much of the buffer has been consumed.
            reader.AdvanceTo(buffer.Start, buffer.End);

            // Stop reading if there's no more data coming
            if (result.IsCompleted) break;
        }
    }

    private string ProcessLine(in ReadOnlySequence<byte> readOnlySequence)
    {
        // Line parsing logic
        return String.Empty;
    }

    // Buffer the incoming data until a new line is found.
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
        
        // Remove the parsed message from the input buffer.
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
    
    private class PrimServiceResponse
    {
        public const string Method = "isPrime";
        public bool IsPrime { get; set; }

        public PrimServiceResponse(bool isPrime)
        {
            IsPrime = isPrime;
        }
    };
}

