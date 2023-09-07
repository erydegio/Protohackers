using System.Text.Json;

namespace ProtoHackers.Problem01;

public class PrimeServiceRequestHandler : IRequestHandler
{
    private JsonSerializerOptions? _jsonOptions =
        new (){ PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    
    public PrimeService.PrimServiceResponse HandleRequest(ReadOnlyMemory<byte> line)
    {
        var request = Deserialize(line);
        return Validate(request);
    }

    private PrimeService.PrimServiceResponse Validate(PrimeService.PrimServiceRequest request)
    {
        if (request.Method != "isPrime")
            throw new MalformedRequestException("Incorrect method.");
        
        return new PrimeService.PrimServiceResponse(request.Method, IsPrime(request.Number));
    }

    PrimeService.PrimServiceRequest? Deserialize(ReadOnlyMemory<byte> segment)
    {
        var utf8Reader = new Utf8JsonReader(segment.Span);
        try
        {
            return JsonSerializer.Deserialize<PrimeService.PrimServiceRequest>(ref utf8Reader, _jsonOptions);
        }
        catch (Exception)
        {
            throw new JsonFormatException("Invalid json format.");
        }
    }
    
    public static bool IsPrime(decimal n, int i=2)
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
}

public class MalformedRequestException : Exception
{
    public MalformedRequestException(string message) : base(message) { }
}

public class JsonFormatException : Exception
{
    public JsonFormatException(string message) : base(message) { }
}