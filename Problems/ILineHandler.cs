using System.Buffers;
using ProtoHackers.Problem01;

namespace ProtoHackers;

public interface ILineHandler<in TResponse>
{
    bool HandleLine(ReadOnlySequence<byte> segment, out PrimServiceResponse response);
    bool TryReadLine(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> line);
    byte[] Serialize(TResponse response);
}