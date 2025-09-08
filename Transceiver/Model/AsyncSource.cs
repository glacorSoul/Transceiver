// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace Transceiver;

public class AsyncSource<T>
{
    private readonly Channel<T> _channel = Channel.CreateUnbounded<T>(new UnboundedChannelOptions
    {
        SingleReader = false,
        SingleWriter = false,
        AllowSynchronousContinuations = true,
    });

    public void Complete()
    {
        _ = _channel.Writer.TryComplete();
    }

    public async IAsyncEnumerable<T> ReadAllAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            T value = await _channel.Reader.ReadAsync(cancellationToken);
            yield return value;
        }
    }

    public async Task WriteAsync(T item)
    {
        await _channel.Writer.WriteAsync(item);
    }
}