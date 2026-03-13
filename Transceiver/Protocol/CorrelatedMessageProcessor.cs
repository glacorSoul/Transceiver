// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using System.Reflection;
using System.Threading.Channels;

namespace Transceiver;

public sealed class CorrelatedMessageProcessor : IMessageProcessor, IDisposable
{
    private readonly CancellationTokenSource _cts = new();

    private readonly ChannelAsyncSource<IIdentifiable> _messages = new(new UnboundedChannelOptions
    {
        SingleReader = true,
        SingleWriter = false,
        AllowSynchronousContinuations = false,
    });

    private readonly Thread _retainedMessagesProcessor;
    private readonly ISerializer _serializer;
    private readonly ConcurrentDictionaryList<Guid, StreamEntry> _streams = new();
    private bool disposedValue;

    public CorrelatedMessageProcessor(ISerializer serializer)
    {
        _serializer = serializer;
        _retainedMessagesProcessor = new(() => ProcessMessages().GetAwaiter().GetResult());
        _retainedMessagesProcessor.Start();
    }

    public IAsyncSource<T> AddRequester<T>(Guid requestId) where T : IIdentifiable
    {
        IAsyncSource<T> result = new ChannelAsyncSource<T>(new UnboundedChannelOptions
        {
            SingleWriter = true,
            SingleReader = true,
            AllowSynchronousContinuations = false
        });
        _streams.Add(requestId, new StreamEntry(result));
        return result;
    }


    public async Task ProcessMessageAsync(TransceiverMessage message, CancellationToken cancellationToken)
    {
        IIdentifiable data = (IIdentifiable)_serializer.Deserialize(message.Header.Type, message.Data);
        await _messages.WriteAsync(data, cancellationToken);
    }

    public async Task ProcessUnserializedMessageAsync<T>(T message, CancellationToken cancellationToken) where T : IIdentifiable
    {
        await _messages.WriteAsync(message, cancellationToken);
    }

    private List<StreamEntry> GetStreams(IIdentifiable message)
    {
        Type messageType = message.GetType();
        List<StreamEntry> streams = _streams.RemoveAll(message.Id, m => m.MessageType == messageType);
        streams.AddRange(_streams.GetAll(Guid.Empty, m => m.MessageType == messageType));
        return streams;
    }

    private async ValueTask ProcessMessages()
    {
        await foreach (IIdentifiable message in _messages.ReadAllAsync(_cts.Token))
        {
            List<StreamEntry> streams = GetStreams(message);
            await SendMessage(message, streams);

            if (streams.Count == 0)
            {
                await _messages.WriteAsync(message, _cts.Token);
            }
        }
    }

    private async ValueTask SendMessage(IIdentifiable message, List<StreamEntry> streams)
    {
        if (streams.Count == 0)
        {
            return;
        }
        Type asyncType = typeof(IAsyncSource<>).MakeGenericType(message.GetType());
        MethodInfo sendDataMethod = asyncType.GetMethod(nameof(IAsyncSource<>.WriteAsync)) ?? default!;
        foreach (StreamEntry stream in streams)
        {
            ValueTask task = (ValueTask)sendDataMethod.Invoke(stream.AsyncSource, [message, _cts.Token])!;
            await task;
        }
    }

    private sealed class StreamEntry
    {
        public StreamEntry(object asyncSouce)
        {
            AsyncSource = asyncSouce;
            MessageType = asyncSouce.GetType().GetGenericArguments()[0];
        }

        public object AsyncSource { get; private set; }
        public Type MessageType { get; private set; }
    }

    private void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _cts.Cancel();
                _cts.Dispose();
            }

            disposedValue = true;
        }
    }

    ~CorrelatedMessageProcessor()
    {
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}