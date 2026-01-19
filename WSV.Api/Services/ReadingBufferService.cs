using System.Diagnostics.CodeAnalysis;
using System.Threading.Channels;
using WSV.Api.Models;


namespace WSV.Api.Services;

public class ReadingBufferService: IReadingBufferService
{
    private readonly Channel<SourceReading> _channel;
    private int _approxCount;
    public int ApproximateCount => Volatile.Read(ref _approxCount);

    public ReadingBufferService(int capacity = 200)
    {
        var options = new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false
        };

        _channel = Channel.CreateBounded<SourceReading>(options);
    }

    public async ValueTask EnqueueAsync(SourceReading reading, CancellationToken ct)
    {
        await _channel.Writer.WriteAsync(reading, ct);
        Interlocked.Increment(ref _approxCount);
    } 

    // Reactive async dequeueing, better but wont introduce lag
    public ValueTask<SourceReading> DequeueAsync(CancellationToken ct)
        => _channel.Reader.ReadAsync(ct);

    // Allows for dequeueing lag
    public bool TryDequeue([NotNullWhen(true)] out SourceReading? reading)
    {
        if (_channel.Reader.TryRead(out reading))
        {
            Interlocked.Decrement(ref _approxCount);
            return true;
        }

        return false;
    }
}