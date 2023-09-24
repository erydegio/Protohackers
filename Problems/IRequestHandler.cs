using ProtoHackers.Problem01;

namespace ProtoHackers;

public interface IRequestHandler
{
    public byte[] HandleRequest(ReadOnlyMemory<byte> line);
}