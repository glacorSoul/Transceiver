// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using System.Collections.Concurrent;

namespace Transceiver.AzureQueue;

public sealed class AzureQueueMessageProcessor : IMessageProcessor, IDisposable
{
    private readonly CancellationTokenSource _cts;
    private readonly CorrelatedMessageProcessor _messageProcessor;
    private readonly Func<string, QueueClient> _queueFactory;
    private readonly ConcurrentDictionary<Type, QueueClient> _queues = new();
    private readonly Thread _receiveMessages;
    private readonly ISerializer _serializer;
    private bool _disposed;

    public AzureQueueMessageProcessor(Func<string, QueueClient> queueFactory, CorrelatedMessageProcessor processor, ISerializer serializer)
    {
        _cts = new();
        _queueFactory = queueFactory;
        _messageProcessor = processor;
        _receiveMessages = new(() => StartReceivingMessagesAsync(_cts.Token).GetAwaiter().GetResult());
        _receiveMessages.Start();
        _serializer = serializer;
    }

    ~AzureQueueMessageProcessor()
    {
        Dispose(false);
    }

    public AsyncSource<T> AddRequester<T>(Guid requestId) where T : IIdentifiable
    {
        return _messageProcessor.AddRequester<T>(requestId);
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

    public Task ProcessMessageAsync(TransceiverMessage message, CancellationToken cancellationToken)
    {
        QueueClient queueClient = CreateQueue(message.Header.Type);
        return queueClient.SendMessageAsync(message.ToCsv(), cancellationToken: cancellationToken);
    }

    private QueueClient CreateQueue(Type type)
    {
        string name = type.ToShortPrettyString().ToLettersOnly().ToLowerInvariant();
        return _queues.GetOrAdd(type, type =>
        {
            QueueClient queueClient = _queueFactory(name);
            _ = queueClient.CreateIfNotExists();
            return queueClient;
        });
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }
        if (disposing)
        {
            _cts.Cancel();
            _cts.Dispose();
            _messageProcessor.Dispose();
        }
        _disposed = true;
    }

    private async Task StartReceivingMessagesAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            IEnumerable<Task> tasks = _queues.Values.Select(async queueClient =>
            {
                QueueMessage[] messages = await queueClient.ReceiveMessagesAsync(10, TimeSpan.FromSeconds(30), cancellationToken);
                foreach (QueueMessage queueMessage in messages)
                {
                    TransceiverMessage message = new(queueMessage.Body.ToString());
                    await _messageProcessor.ProcessMessageAsync(message, cancellationToken);
                    _ = await queueClient.DeleteMessageAsync(queueMessage.MessageId, queueMessage.PopReceipt, cancellationToken);
                }
            });
            await Task.WhenAll(tasks);
        }
    }
}