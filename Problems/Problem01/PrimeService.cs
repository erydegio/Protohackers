using System.Buffers;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Text;
using ProtoHackers;
using ProtoHackers.Problem01;

namespace Protohackers.Problem01;

public class PrimeService : ITcpService
{
    private readonly ILineHandler<PrimeServiceRequest, PrimeServiceResponse>? _lineHandler;
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
                _isMalformed = HandleRequest(line, out PrimeServiceResponse response);
                
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

    private bool HandleRequest(ReadOnlySequence<byte> line, out PrimeServiceResponse response)
    {
        var request = _lineHandler!.Deserialize(line);
        response = Validate(request);
        return response.Method == "malformed";
    }
    
    private PrimeServiceResponse Validate(PrimeServiceRequest? request)
    {
        if (request is null || request.Method != "isPrime" || request.Number is null)
            return new PrimeServiceResponse("malformed", false);
        
        if (request.BigNumber)
            return new PrimeServiceResponse(request.Method, false);
        
        return new PrimeServiceResponse(request.Method, IsPrime((long)request.Number.Value));

        bool IsPrime(long n)
        {
            if (!IsInteger(n))
                return false;
            if (n == 2)
                return true;
            if (n < 2 || n % 2 == 0)
                return false;

            int sqrt = (int)Math.Sqrt(n);
            for (int divisor = 3; divisor <= sqrt; divisor += 2)
            {
                if (n % divisor == 0)
                    return false;
            }

            return true;

            bool IsInteger(decimal input)
            {
                // Check if the decimal number has no fractional part
                return input == Math.Floor(input);
            }
        }
    }
}