using System.Buffers;
using System.Text;
using System.Text.Json;

namespace ProtoHackers.Problem01;

public class PrimeLineHandler : ILineHandler<PrimeServiceRequest, PrimeServiceResponse?>
{
    private readonly JsonSerializerOptions? _jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private const byte Delimiter = 10;
    
    public PrimeServiceRequest? Deserialize(ReadOnlySequence<byte> segment)
    {
        var utf8Reader = new Utf8JsonReader(segment);
        try
        {
            return JsonSerializer.Deserialize<PrimeServiceRequest>(ref utf8Reader, _jsonOptions);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return null;
        }
    }
    
    public bool TryReadLine(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> line)
    {
        SequencePosition? position = buffer.PositionOf(Delimiter);

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
    
    public byte[] Serialize(PrimeServiceResponse response)
    {
        var json = JsonSerializer.Serialize(response, _jsonOptions);
        var jsonResponse = Encoding.ASCII.GetBytes(json);
        
        Array.Resize(ref jsonResponse, jsonResponse.Length + 1);
        jsonResponse[^1] = Delimiter;
        
        return jsonResponse;
    }
    
}
public record PrimeServiceResponse(string Method, bool Prime);
public record PrimeServiceRequest(string Method, double? Number, bool BigNumber);




