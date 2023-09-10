using System.Text;
using System.Text.Json;

namespace ProtoHackers.Problem01;

public class PrimeServiceRequestHandler : IRequestHandler
{
    private JsonSerializerOptions? _jsonOptions =
        new (){ PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    
    public PrimServiceResponse HandleRequest(ReadOnlyMemory<byte> line)
    {
        var request = Deserialize(line);
        return Validate(request);
        
    }

    private PrimServiceResponse Validate(PrimServiceRequest request)
    {
        if (request.Method != "isPrime")
            throw new MalformedRequestException("Incorrect method.");
        return new PrimServiceResponse(request.Method, IsPrime(request.Number));
    }

    PrimServiceRequest? Deserialize(ReadOnlyMemory<byte> segment)
    {
        var utf8Reader = new Utf8JsonReader(segment.Span);
        try
        {
            return JsonSerializer.Deserialize<PrimServiceRequest>(ref utf8Reader, _jsonOptions);
        }
        catch (Exception e)
        {
            throw new JsonFormatException("Invalid json format.");
        }
    }
    
    public static bool IsPrime(decimal n)
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

public class MalformedRequestException : Exception
{
    public MalformedRequestException(string message) : base(message) { }
}

public class JsonFormatException : Exception
{
    public JsonFormatException(string message) : base(message) { }
}

public record PrimServiceResponse(string Method, bool Prime);
public record PrimServiceRequest(string Method, long Number);