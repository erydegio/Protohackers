using ProtoHackers.Problem01;

namespace ProtoHackers;

public interface IRequestHandler
{
    public PrimeService.PrimServiceResponse HandleRequest(ReadOnlyMemory<byte> line);
}