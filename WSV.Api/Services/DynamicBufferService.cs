using System.Threading.Channels;
using Microsoft.Extensions.Options;
using WSV.Api.Models;
using WSV.Api.Configuration;

namespace WSV.Api.Services;

public class DynamicBufferService : IDynamicBufferService
{
    private readonly Channel<SourceReading> _primary;
    private readonly List<Channel<SourceReading>> _overflowChannels = new();

    private readonly int _capacityPrimary;
    private readonly int _capacityOverflow;
    private readonly int _maxOverflowChannels;

    private const double ExpandThreshold = 0.6;
    private const double OverflowShrinkThreshold = 0.5;
    private const double PrimaryShrinkThreshold = 0.75;
    
    private readonly object _lock = new();

    private long _droppedCount = 0;
    private readonly ILogger<DynamicBufferService> _logger;

    public DynamicBufferService(
        ILogger<DynamicBufferService> logger,
        IOptions<BufferOptions> options)
    {
        _logger = logger;
        var opt = options.Value;
        _capacityPrimary = opt.CapacityPrimary;
        _capacityOverflow = opt.CapacityOverflow;
        _maxOverflowChannels = opt.MaxOverflowChannels;

        _primary = Channel.CreateBounded<SourceReading>(new BoundedChannelOptions(_capacityPrimary)
        {
            FullMode = BoundedChannelFullMode.DropWrite,
            SingleReader = true,
            SingleWriter = false
        });
    }

    public int BufferedCount
    {
        get
        {
            lock (_lock)
            {
                int total = _primary.Reader.Count + _overflowChannels.Sum(c => c.Reader.Count);
                return total;
            }
        }
    }
    
    public void Enqueue(SourceReading reading)
    {
        lock (_lock)
        {
            if((_overflowChannels.Count == 0 && _primary.Reader.Count >= ExpandThreshold * _capacityPrimary) ||
            (_overflowChannels.Count > 0 && _overflowChannels[^1].Reader.Count >= ExpandThreshold * _capacityOverflow))
                TryExpand();
        
            if(_primary.Reader.Count < _capacityPrimary)
                _primary.Writer.TryWrite(reading);
            else
            {
                foreach(var channel in _overflowChannels)
                {
                    if(channel.Reader.Count < _capacityOverflow)
                    {
                        channel.Writer.TryWrite(reading);
                        return;
                    }   
                }
                    
                _droppedCount ++;
                int currentCount = _primary.Reader.Count + _overflowChannels.Sum(c => c.Reader.Count);
                _logger.LogWarning("Reading dropped - buffer at full capacity. Total dropped: {dropped}. Buffered count: {buffered}/{capacity}.",
                _droppedCount,
                currentCount,
                _capacityPrimary + (_overflowChannels.Count * _capacityOverflow));
            }
        }
    }

    public SourceReading? Dequeue()
    {
        lock (_lock)
        {
            _primary.Reader.TryRead(out SourceReading? reading);
            TryBackfill();
            TryShrink();
            return reading;
        }
    }

    private void TryExpand()
    {
        if(_overflowChannels.Count >= _maxOverflowChannels)
        {
            _logger.LogWarning("Overflow channel couldnt be created, maximum channels reached {channels}/{maximum}. Data loss imminent", 
            _overflowChannels.Count, _maxOverflowChannels);
            return;
        }
        
        _overflowChannels.Add(Channel.CreateBounded<SourceReading>(new BoundedChannelOptions(_capacityOverflow)
        {
            FullMode = BoundedChannelFullMode.DropWrite,
            SingleReader = true,
            SingleWriter = false
        }));
        _logger.LogInformation("Overflow channel #{number} created. Total buffer capacity now {total}.",
        _overflowChannels.Count, _capacityPrimary + (_capacityOverflow * _overflowChannels.Count));
    }

    private void TryBackfill()
    {
        if(_overflowChannels.Count > 0)
        {
            while(_primary.Reader.Count != _capacityPrimary && _overflowChannels[0].Reader.Count != 0)
            {
                _overflowChannels[0].Reader.TryRead(out SourceReading? reading);
                if(reading is not null)
                    _primary.Writer.TryWrite(reading);
            }
        }
        if(_overflowChannels.Count > 1)
        {
            for(int i = 0; i < (_overflowChannels.Count - 1); i++)
            {
                while(_overflowChannels[i].Reader.Count != _capacityOverflow && _overflowChannels[i+1].Reader.Count != 0)
                {
                    _overflowChannels[i+1].Reader.TryRead(out SourceReading? reading);
                    if(reading is not null)
                        _overflowChannels[i].Writer.TryWrite(reading);
                }
            }
        }
    }

    private void TryShrink()
    {
        if(_overflowChannels.Count > 0)
            {
                while(_overflowChannels.Count > 1)
                {
                    if(_overflowChannels[^1].Reader.Count == 0 && _overflowChannels[^2].Reader.Count <= OverflowShrinkThreshold * _capacityOverflow)   
                    {
                        int channelNumber = _overflowChannels.Count;
                        _overflowChannels.RemoveAt(_overflowChannels.Count - 1);
                        _logger.LogInformation("Overflow channel #{number} was deleted. Total buffer capacity now {total}.",
                        channelNumber, _capacityPrimary + (_capacityOverflow * _overflowChannels.Count));
                    }
                        
                    else
                        return;
                }

                if(_overflowChannels[0].Reader.Count == 0 && _primary.Reader.Count <= PrimaryShrinkThreshold * _capacityPrimary)
                {
                    int channelNumber = _overflowChannels.Count;
                    _overflowChannels.RemoveAt(_overflowChannels.Count - 1);
                    _logger.LogInformation("Overflow channel #{number} was deleted. Total buffer capacity now {total}.",
                    channelNumber, _capacityPrimary + (_capacityOverflow * _overflowChannels.Count));
                }
                    
            }
    }
}