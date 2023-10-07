using System.Buffers;
using System.Text;
using System.Text.Json;

namespace ProtoHackers.Problem01;

public class PrimeLineHandler : ILineHandler<PrimServiceResponse>
{
    private readonly JsonSerializerOptions? _jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private const byte Delimiter = 10;

    public bool HandleLine(ReadOnlySequence<byte> line, out PrimServiceResponse response)
    {
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
    
    public byte[] Serialize(PrimServiceResponse response)
    {
        var json = JsonSerializer.Serialize(response, _jsonOptions);
        var jsonResponse = Encoding.ASCII.GetBytes(json);
        
        Array.Resize(ref jsonResponse, jsonResponse.Length + 1);
        jsonResponse[^1] = Delimiter;
        
        Console.WriteLine(Encoding.UTF8.GetString(jsonResponse));
        return jsonResponse;
    }
    
    private record PrimServiceRequest(string Method, double? Number, bool BigNumber);
}
public record PrimServiceResponse(string Method, bool Prime);



