// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using System.Reflection;

namespace Transceiver;

public sealed class CorrelatedMessageProcessor : IMessageProcessor, IDisposable
{
    private readonly CancellationTokenSource _cts = new();
    private readonly AsyncSource<IIdentifiable> _messages = new();
    private readonly Thread _retainedMessagesProcessor;
    private readonly ISerializer _serializer;
    private readonly ConcurrentDictionaryList<Guid, object> _streams = new();

    public CorrelatedMessageProcessor(ISerializer serializer)
    {
        _serializer = serializer;
        _retainedMessagesProcessor = new(async () => await ProcessMessages());
        _retainedMessagesProcessor.Start();
    }

    public AsyncSource<T> AddRequester<T>(Guid requestId) where T : IIdentifiable
    {
        AsyncSource<T> result = new();
        _streams.Add(requestId, result);
        return result;
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }

    public async Task ProcessGenericMessageAsync<T>(T data, CancellationToken cancellationToken) where T : IIdentifiable
    {
        await _messages.WriteAsync(data);
    }

    public async Task ProcessMessageAsync(TransceiverMessage message, CancellationToken cancellationToken)
    {
        IIdentifiable data = (IIdentifiable)_serializer.Deserialize(message.Header.Type, message.Data);
        await _messages.WriteAsync(data);
    }

    private async Task ProcessMessages()
    {
        await foreach (IIdentifiable message in _messages.ReadAllAsync(_cts.Token))
        {
            Type asyncType = typeof(AsyncSource<>).MakeGenericType(message.GetType());
            MethodInfo sendDataMethod = asyncType.GetMethod(nameof(AsyncSource<string>.WriteAsync))!;

            List<object> streams = _streams.RemoveAll(message.Id, m => m.GetType().GetGenericArguments()[0] == message.GetType());
            if (streams.Count == 0)
            {
                streams = _streams.GetAll(Guid.Empty, m => m.GetType().GetGenericArguments()[0] == message.GetType());
            }
            if (streams.Count == 0)
            {
                _ = Task.Delay(TimeSpan.FromMilliseconds(0))
                    .ContinueWith((ctx) => _messages.WriteAsync(message));
            }
            foreach (object stream in streams)
            {
                _ = sendDataMethod.Invoke(stream, [message]);
            }
        }
    }
}