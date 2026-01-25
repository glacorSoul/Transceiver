// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace Transceiver;

public class ChannelAsyncSource<T> : IAsyncSource<T>
{
    private readonly Channel<T> _channel;

    public ChannelAsyncSource() : this(new UnboundedChannelOptions()
    {
        SingleReader = false,
        SingleWriter = false,
        AllowSynchronousContinuations = false,
    })
    {
    }

    public ChannelAsyncSource(UnboundedChannelOptions options)
    {
        _channel = Channel.CreateUnbounded<T>(options);
    }

    public void Complete()
    {
        _ = _channel.Writer.TryComplete();
    }

    public async IAsyncEnumerable<T> ReadAllAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        while (await _channel.Reader.WaitToReadAsync(cancellationToken).ConfigureAwait(false))
        {
            while (_channel.Reader.TryRead(out T? item))
            {
                yield return item;
            }
        }
    }

    public ValueTask WriteAsync(T item, CancellationToken cancellationToken)
    {
        if (!_channel.Writer.TryWrite(item))
        {
            return _channel.Writer.WriteAsync(item, cancellationToken);
        }
        return new ValueTask(Task.CompletedTask);
    }
}