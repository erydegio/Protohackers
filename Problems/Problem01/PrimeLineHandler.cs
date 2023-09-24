using System.Buffers;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Unicode;

namespace ProtoHackers.Problem01;

public class PrimeLineHandler : ILineHandler<PrimServiceResponse>
{
    private static JsonSerializerOptions? _jsonOptions =
        new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    

    public bool HandleLine(ReadOnlyMemory<byte> segment, out PrimServiceResponse response)
    {
        var request = Deserialize(segment);
        response = Validate(request);
        return response.Method == "malformed";
    }

    private PrimServiceRequest? Deserialize(ReadOnlyMemory<byte> segment)
    {
        var utf8Reader = new Utf8JsonReader(segment.Span);
        try
        {
            return JsonSerializer.Deserialize<PrimServiceRequest>(ref utf8Reader, _jsonOptions);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return null;
        }
    }

    private PrimServiceResponse Validate(PrimServiceRequest? request)
    {
        if (request is null || request.Method != "isPrime")
            return new PrimServiceResponse("malformed", false);

        return new PrimServiceResponse(request.Method, IsPrime(request.Number));

        bool IsPrime(decimal n)
        {
            if (!IsInteger(n))
                return false;
            if (n == 2)
                return true;
            if (n < 2 || n % 2 == 0)
                return false;

            int sqrt = (int)Math.Sqrt((double)n);
            for (int divisor = 3; divisor <= sqrt; divisor += 2)
            {
                if (n % divisor == 0)
                    return false;
            }

            return true;

            bool IsInteger(decimal input)
            {
                // Check if the decimal number has no fractional part
                return input == Math.Floor(input);
            }
        }
    }
    
    public byte[] Serialize(PrimServiceResponse response)
    {
        var json = JsonSerializer.Serialize(response, _jsonOptions);
        var delimitedResponse = $"{json}\n";
        
        return Encoding.ASCII.GetBytes(delimitedResponse);
    }


    public bool TryReadLine(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> line)
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

    private record PrimServiceRequest(string Method, long Number);
}
public record PrimServiceResponse(string Method, bool Prime);



