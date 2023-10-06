using System.Buffers;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using ProtoHackers;
using ProtoHackers.Problem01;

namespace Protohackers.Problem01;

public class PrimeService : ITcpService
{
    private PrimeLineHandler? _lineHandler;
    private PipeReader? _reader;

    public PrimeService() =>  _lineHandler = new PrimeLineHandler();

    public async Task HandleClient(Socket socket)
    {
        Console.WriteLine("Service Handling client....");
        var stream = new NetworkStream(socket, true);
        _reader = PipeReader.Create(stream);
        var isMalformed = false;

        while (!isMalformed)
        {
            ReadResult result = await _reader.ReadAsync();
            ReadOnlySequence<byte> buffer = result.Buffer;

            while (_lineHandler!.TryReadLine(ref buffer, out ReadOnlySequence<byte> line))
            {
                isMalformed = _lineHandler.HandleLine(line, out PrimServiceResponse response);
                
                Console.WriteLine($"response ->{response.Method} {response.Prime} from line: {DecodeReadOnlySequence(line, Encoding.UTF8)}");
                
                await stream.WriteAsync(Serialize(response));

                if (isMalformed) 
                    break;
            }

            _reader.AdvanceTo(buffer.Start, buffer.End);

            // Stop reading if there's no more data coming
            if (result.IsCompleted) 
                break;
        }

        await _reader.CompleteAsync();
        socket.Close();
    }

    public byte[] Serialize(PrimServiceResponse response)
    {
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        var delimitedResponse = $"{json}\n"; 
        
        return Encoding.ASCII.GetBytes(delimitedResponse);
    }
    
    // Used to log
    public static string DecodeReadOnlySequence(ReadOnlySequence<byte> byteSequence, Encoding encoding)
    {
        var stringBuilder = new StringBuilder();

        foreach (var memorySegment in byteSequence)
        {
            string segmentString = encoding.GetString(memorySegment.Span);
            stringBuilder.Append(segmentString);
        }
        return stringBuilder.ToString();
    }
}