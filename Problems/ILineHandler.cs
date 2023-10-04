using System.Buffers;
using System.Net.Sockets;
using ProtoHackers.Problem01;

namespace ProtoHackers;

public interface ILineHandler<TResponse>
{
    public bool HandleLine(ReadOnlySequence<byte> segment, out PrimServiceResponse response);
    bool TryReadLine(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> line);
}

public class LineHandler
{
    
}