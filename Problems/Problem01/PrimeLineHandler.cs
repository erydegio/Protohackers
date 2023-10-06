using System.Buffers;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Unicode;
using Microsoft.VisualBasic.CompilerServices;
using Protohackers.Problem01;

namespace ProtoHackers.Problem01;

public class PrimeLineHandler : ILineHandler<PrimServiceResponse>
{
    private static JsonSerializerOptions? _jsonOptions =
        new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    
    public bool HandleLine(ReadOnlySequence<byte> line, out PrimServiceResponse response)
    {
        Console.WriteLine(PrimeService.DecodeReadOnlySequence(line, Encoding.UTF8));
        var request = Deserialize(line);
        response = Validate(request);
        return response.Method == "malformed";
    }

    private PrimServiceRequest? Deserialize(ReadOnlySequence<byte> segment)
    {
        var utf8Reader = new Utf8JsonReader(segment);
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
        if (request is null || request.Method != "isPrime" || request.Number is null)
            return new PrimServiceResponse("malformed", false);
        
        if (request.BigNumber)
            return new PrimServiceResponse(request.Method, false);
        
        return new PrimServiceResponse(request.Method, IsPrime((long)request.Number.Value));

        bool IsPrime(long n)
        {
            if (!IsInteger(n))
                return false;
            if (n == 2)
                return true;
            if (n < 2 || n % 2 == 0)
                return false;

            int sqrt = (int)Math.Sqrt(n);
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

    private record PrimServiceRequest(string Method, double? Number, bool BigNumber);
}
public record PrimServiceResponse(string Method, bool Prime);



