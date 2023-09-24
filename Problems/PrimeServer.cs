using System.Buffers;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using ProtoHackers.Problem01;

namespace ProtoHackers;

public class PrimeServer : TcpServer
{
    private ILineHandler<PrimServiceResponse> _lineHandler;
    protected override async Task HandleConnection(Socket socket)
    {
        var stream = new NetworkStream(socket, true);
        var reader = PipeReader.Create(stream);
        var writer = new Utf8JsonWriter(stream);

        while (true) // stop malformed
        {
            ReadResult result = await reader.ReadAsync();
            ReadOnlySequence<byte> buffer = result.Buffer;

            while (_lineHandler.TryReadLine(ref buffer, out ReadOnlySequence<byte> line))
            {
                foreach (var segment in line)
                {
                    var isMalformed = _lineHandler.HandleLine(segment, out PrimServiceResponse response);

                    //stop if malformed response is written
                    await stream.WriteAsync(_lineHandler.Serialize(response)); // move logic to server
                    
                    if (isMalformed)
                        break;
                }
            }

            reader.AdvanceTo(buffer.Start, buffer.End);

            // Stop reading if there's no more data coming.
            if (result.IsCompleted)
                break;
        }

        await reader.CompleteAsync();
    }
    
    public PrimeServer(ILoggerFactory loggerFactory, ILineHandler<PrimServiceResponse> lineHandler) : base(loggerFactory)
    {
        _lineHandler = lineHandler;
    }
}