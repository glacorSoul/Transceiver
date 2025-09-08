// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Transceiver.RabbitMq;

public sealed class RabbitMqMessageProcessor : IMessageProcessor, IDisposable
{
    private readonly Lazy<Task<IConnection>> _connection;
    private readonly HashSet<Type> _consumerTypes;
    private readonly CancellationTokenSource _cts;
    private readonly CorrelatedMessageProcessor _processor;
    private readonly HashSet<Type> _queueTypes;
    private readonly ISerializer _serializer;
    private bool _disposed;

    public RabbitMqMessageProcessor(ConnectionFactory connectionFactory, CorrelatedMessageProcessor processor, ISerializer serializer)
    {
        _cts = new();
        _connection = new(() => connectionFactory.CreateConnectionAsync(_cts.Token));
        _processor = processor;
        _consumerTypes = [];
        _queueTypes = [];
        _serializer = serializer;
    }

    ~RabbitMqMessageProcessor()
    {
        Dispose(false);
    }

    public AsyncSource<T> AddRequester<T>(Guid requestId) where T : IIdentifiable
    {
        AsyncSource<T> result = _processor.AddRequester<T>(requestId);
        CreateQueueAsync(typeof(T), null, CancellationToken.None).GetAwaiter().GetResult();
        _ = CreateConsumerAsync(typeof(T), CancellationToken.None);
        return result;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public Task ProcessGenericMessageAsync<T>(T data, CancellationToken cancellationToken) where T : IIdentifiable
    {
        return ProcessMessageAsync(new TransceiverMessage(data, _serializer), cancellationToken);
    }

    public async Task ProcessMessageAsync(TransceiverMessage message, CancellationToken cancellationToken)
    {
        await CreateQueueAsync(message.Header.Type, message, cancellationToken);
        _ = CreateConsumerAsync(message.Header.Type, cancellationToken);
    }

    private async Task CreateConsumerAsync(Type type, CancellationToken cancellationToken)
    {
        bool isNewConsumer = false;
        lock (_consumerTypes)
        {
            isNewConsumer = _consumerTypes.Add(type);
        }
        if (!isNewConsumer)
        {
            return;
        }

        string queueName = type.ToShortPrettyString().ToLettersOnly().ToLowerInvariant();
        IConnection connection = await _connection.Value;
        using IChannel channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
        AsyncEventingBasicConsumer consumer = new(channel);
        consumer.ReceivedAsync += (sender, ea) =>
        {
            TransceiverMessage receivedMessage = new(ea.Body.ToArray());
            return _processor.ProcessMessageAsync(receivedMessage, cancellationToken);
        };
        _ = await channel.BasicConsumeAsync(queueName, true, consumer, cancellationToken);
        _ = cancellationToken.WaitHandle.WaitOne();
    }

    private async Task CreateQueueAsync(Type type, TransceiverMessage? message, CancellationToken cancellationToken)
    {
        string queueName = type.ToShortPrettyString().ToLettersOnly().ToLowerInvariant();
        IConnection connection = await _connection.Value;
        using IChannel channel = await connection.CreateChannelAsync();
        bool isNewQueueType = false;
        lock (_queueTypes)
        {
            isNewQueueType = _queueTypes.Add(type);
        }
        if (isNewQueueType)
        {
            _ = await channel.QueueDeclareAsync(queueName, durable: true, exclusive: false, autoDelete: false);
        }
        if (message is not null)
        {
            await channel.BasicPublishAsync("", queueName, message.ToBytes(), cancellationToken);
        }
    }

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _cts.Cancel();
                _cts.Dispose();
                _connection.Value.Dispose();
            }
            _disposed = true;
        }
    }
}