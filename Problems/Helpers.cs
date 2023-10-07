using System.Buffers;
using System.Text;

namespace ProtoHackers;

public static class Helpers
{
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