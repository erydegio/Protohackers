using System.Buffers;
using ProtoHackers.Problem01;

namespace ProtoHackers;

public interface ILineHandler<out TRequest, in TResponse>
{
    bool TryReadLine(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> line);
    byte[] Serialize(TResponse response);
    TRequest? Deserialize(ReadOnlySequence<byte> segment);
}