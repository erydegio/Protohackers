using ProtoHackers.Problem01;

namespace ProtoHackers;

public interface IRequestHandler
{
    public PrimServiceResponse HandleRequest(ReadOnlyMemory<byte> line);
}