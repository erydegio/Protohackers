using System.Buffers;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Text;
using ProtoHackers;
using ProtoHackers.Problem01;

namespace Protohackers.Problem01;

public class PrimeService : ITcpService
{
    private ILineHandler<PrimServiceResponse>? _lineHandler;
    private PipeReader? _reader;
    private NetworkStream? _stream;
    private bool _isMalformed;

    public PrimeService() =>  _lineHandler = new PrimeLineHandler();

    public async Task HandleClient(Socket socket)
    {
        _stream = new NetworkStream(socket, true);
        _reader = PipeReader.Create(_stream);

        while (!_isMalformed)
        {
            ReadResult result = await _reader.ReadAsync();
            ReadOnlySequence<byte> buffer = result.Buffer;

            while (_lineHandler!.TryReadLine(ref buffer, out ReadOnlySequence<byte> line))
            {
                _isMalformed = _lineHandler.HandleLine(line, out PrimServiceResponse response);
                
                Console.WriteLine($"response ->{response.Method} {response.Prime} from line: {Helpers.DecodeReadOnlySequence(line, Encoding.UTF8)}");
                
                await _stream.WriteAsync(_lineHandler.Serialize(response));

                if (_isMalformed) 
                    break;
            }

            _reader.AdvanceTo(buffer.Start, buffer.End);

            // Stop reading if there's no more data coming
            if (result.IsCompleted) 
                break;
        }

        await _reader.CompleteAsync();
        socket.Close();
    }
}