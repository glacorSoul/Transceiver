// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using System.Collections.Concurrent;
using System.Reflection;
using System.Threading.Channels;

namespace Transceiver;

public sealed class CorrelatedMessageProcessor : IMessageProcessor, IDisposable
{
    private readonly CancellationTokenSource _cts = new();
    private readonly List<StreamEntry> _streamsBuffer = [];
    private readonly ChannelAsyncSource<IIdentifiable> _messages = new(new UnboundedChannelOptions
    {
        SingleReader = true,
        SingleWriter = false,
        AllowSynchronousContinuations = false,
    });

    private readonly ISerializer _serializer;
    private readonly ConcurrentDictionaryList<Guid, StreamEntry> _streams = new();
    private bool disposedValue;

    public CorrelatedMessageProcessor(ISerializer serializer)
    {
        _serializer = serializer;
        _ = Task.Run(ProcessMessages, _cts.Token);
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

    public void RemoveRequester<T>(Guid requestId, IAsyncSource<T> source) where T : IIdentifiable
    {
        _ = _streams.RemoveWhere(requestId, new StreamEntryReferenceMatch(source));
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
        Type lastGenericArgument = StreamEntry.GetLastGenericArgument(messageType);
        List<StreamEntry> streams = _streams.GetAll(Guid.Empty, message.Id, new StreamTypeMatch(messageType, lastGenericArgument), _streamsBuffer);
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
        foreach (StreamEntry stream in streams)
        {
            await stream.WriteAsyncInvoker(stream.AsyncSource, message, _cts.Token);
        }
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

    private readonly struct StreamTypeMatch(Type messageType, Type lastGenericArgument) : IPredicate<StreamEntry>
    {
        public bool Matches(StreamEntry value)
        {
            return value.IsOfSameType(messageType, lastGenericArgument);
        }
    }

    private readonly struct StreamEntryReferenceMatch(object asyncSource) : IPredicate<StreamEntry>
    {
        public bool Matches(StreamEntry value)
        {
            return ReferenceEquals(value.AsyncSource, asyncSource);
        }
    }

    private readonly struct StreamEntry
    {
        public StreamEntry(object asyncSouce)
        {
            AsyncSource = asyncSouce;
            MessageType = asyncSouce.GetType().GetGenericArguments()[0];
            MessageTypeLastGenericArgument = GetLastGenericArgument(MessageType);
            WriteAsyncInvoker = GetWriteAsyncInvoker(MessageType);
        }

        public object AsyncSource { get; }
        public Type MessageType { get; }
        public Type MessageTypeLastGenericArgument { get; }
        public Func<object, IIdentifiable, CancellationToken, ValueTask> WriteAsyncInvoker { get; }

        public bool IsOfSameType(Type type, Type typeLastGenericArgument)
        {
            if (type == MessageType)
            {
                return true;
            }

            return MessageTypeLastGenericArgument == typeLastGenericArgument && MessageType.IsAssignableFrom(type);
        }

        private static readonly ConcurrentDictionary<Type, Type> s_lastGenericArgumentCache = new();

        public static Type GetLastGenericArgument(Type type)
        {
            return s_lastGenericArgumentCache.GetOrAdd(type, static t =>
            {
                Type[] genericArguments = t.GenericTypeArguments;
                return genericArguments[genericArguments.Length - 1];
            });
        }

        private static readonly MethodInfo s_invokeWriteAsyncDefinition =
            typeof(StreamEntry).GetMethod(nameof(InvokeWriteAsync), BindingFlags.Public | BindingFlags.Static)!;

        private static Func<object, IIdentifiable, CancellationToken, ValueTask> GetWriteAsyncInvoker(Type messageType)
        {
            MethodInfo closedMethod = s_invokeWriteAsyncDefinition.MakeGenericMethod(messageType);
            return (Func<object, IIdentifiable, CancellationToken, ValueTask>)closedMethod
                .CreateDelegate(typeof(Func<object, IIdentifiable, CancellationToken, ValueTask>));
        }

        public static ValueTask InvokeWriteAsync<T>(object asyncSource, IIdentifiable message, CancellationToken cancellationToken)
            where T : IIdentifiable
        {
            return ((IAsyncSource<T>)asyncSource).WriteAsync((T)message, cancellationToken);
        }
    }

}