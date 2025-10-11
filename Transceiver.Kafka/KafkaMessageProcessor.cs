// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Logging;

namespace Transceiver.Kafka;

public sealed class KafkaMessageProcessor : IMessageProcessor, IDisposable
{
    private readonly AdminClientConfig _adminConfig;
    private readonly ConsumerConfig _consumerConfig;
    private readonly CancellationTokenSource _cts;
    private readonly ILogger<KafkaMessageProcessor> _logger;
    private readonly CorrelatedMessageProcessor _processor;
    private readonly ProducerConfig _producerConfig;
    private readonly Thread _receiveMessages;
    private readonly ISerializer _serializer;
    private readonly HashSet<string> _topics;
    private readonly AsyncSource<string> _topicsSource;
    private bool _disposed;

    public KafkaMessageProcessor(AdminClientConfig adminConfig,
        ProducerConfig producerConfig,
        ConsumerConfig consumerConfig,
        CorrelatedMessageProcessor processor,
        ILogger<KafkaMessageProcessor> logger,
        ISerializer serializer)
    {
        _cts = new();
        _topics = [];
        _topicsSource = new();
        _logger = logger;
        _adminConfig = adminConfig;
        _producerConfig = producerConfig;
        _consumerConfig = consumerConfig;
        _processor = processor;
        _receiveMessages = new(() => ReceiveMessages(_cts.Token).GetAwaiter().GetResult());
        _receiveMessages.Start();
        _serializer = serializer;
    }

    ~KafkaMessageProcessor()
    {
        Dispose(false);
    }

    public AsyncSource<T> AddRequester<T>(Guid requestId) where T : IIdentifiable
    {
        return _processor.AddRequester<T>(requestId);
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
        string topic = await CreateTopicAsync(message.Header.Type);
        using IProducer<Null, byte[]> producer = new ProducerBuilder<Null, byte[]>(_producerConfig).Build();
        _ = await producer.ProduceAsync(topic, new Message<Null, byte[]> { Value = message.ToBytes() }, cancellationToken);
    }

    private async Task<string> CreateTopicAsync(Type type)
    {
        string topic = type.ToShortPrettyString().ToLettersOnly().ToLowerInvariant();
        bool isNewTopic = false;
        lock (_topics)
        {
            isNewTopic = _topics.Add(topic);
        }
        if (!isNewTopic)
        {
            return topic;
        }
        using IAdminClient adminClient = new AdminClientBuilder(_adminConfig).Build();
        try
        {
            await adminClient.CreateTopicsAsync([
                new()
                {
                    Name = topic,
                    NumPartitions = 1,
                    ReplicationFactor = 1,
                }
            ]);
        }
        catch (CreateTopicsException e)
        {
            _logger.LogError(e, "Error creating topic {Topic}", topic);
        }
        await _topicsSource.WriteAsync(topic, _cts.Token);
        return topic;
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
        }
        _disposed = true;
    }

    private async Task ReceiveMessages(CancellationToken cancellationToken)
    {
        await foreach (string topic in _topicsSource.ReadAllAsync(cancellationToken))
        {
            _ = Task.Run(async () =>
            {
                using IConsumer<Null, byte[]> consumer = new ConsumerBuilder<Null, byte[]>(_consumerConfig).Build();
                consumer.Subscribe(topic);
                while (!cancellationToken.IsCancellationRequested)
                {
                    ConsumeResult<Null, byte[]> result = consumer.Consume(TimeSpan.FromSeconds(1));
                    if (result is null || result.Message is null || result.Message.Value is null)
                    {
                        continue;
                    }
                    TransceiverMessage message = new(result.Message.Value);
                    await _processor.ProcessMessageAsync(message, cancellationToken);
                }
                consumer.Close();
            }, cancellationToken);
        }
    }
}