// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using Google.Cloud.PubSub.V1;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace Transceiver.GooglePubSub;

public sealed class GooglePubSubMessageProcessor : IMessageProcessor, IDisposable
{
    private readonly GooglePubSubConfig _config;
    private readonly CancellationTokenSource _cts;
    private readonly ILogger<GooglePubSubMessageProcessor> _logger;
    private readonly CorrelatedMessageProcessor _messageProcessor;
    private readonly Thread _receiveMessages;
    private readonly ISerializer _serializer;
    private readonly HashSet<Type> _topics;
    private readonly AsyncSource<string> _topicsSource;
    private bool _disposed;

    public GooglePubSubMessageProcessor(GooglePubSubConfig config,
        CorrelatedMessageProcessor messageProcessor,
        ILogger<GooglePubSubMessageProcessor> logger,
        ISerializer serializer)
    {
        _cts = new();
        _logger = logger;
        _topics = [];
        _topicsSource = new();
        _config = config;
        _messageProcessor = messageProcessor;
        _receiveMessages = new(() => ReceiveMessagesAsync(_cts.Token).GetAwaiter().GetResult());
        _receiveMessages.Start();
        _serializer = serializer;
    }

    ~GooglePubSubMessageProcessor()
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
        return CreateQueueAsync(message.Header.Type, message, cancellationToken);
    }

    private async Task CreateQueueAsync(Type type, TransceiverMessage? message, CancellationToken cancellationToken)
    {
        string topicId = type.ToShortPrettyString().ToLettersOnly().ToLowerInvariant();
        bool isNewTopic = false;
        lock (_topics)
        {
            isNewTopic = _topics.Add(type);
        }
        TopicName topicName = new(_config.ProjectId, topicId);
        PublisherServiceApiClient publisherClient = await PublisherServiceApiClient.CreateAsync(cancellationToken);
        if (isNewTopic)
        {
            try
            {
                _ = await publisherClient.CreateTopicAsync(topicName);
            }
            catch (RpcException ex)
            {
                _logger.LogWarning(ex, "Failed to create topic {TopicId}. It probably already existed.", topicId);
            }
            await _topicsSource.WriteAsync(topicId, _cts.Token);
        }
        if (message is null)
        {
            return;
        }
        PublishRequest request = new();
        request.Messages.Add(new PubsubMessage
        {
            Data = Google.Protobuf.ByteString.CopyFrom(message.ToBytes()),
        });
        request.TopicAsTopicName = topicName;
        _ = await publisherClient.PublishAsync(request, cancellationToken);
    }

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _cts.Cancel();
                _cts.Dispose();
            }
            _disposed = true;
        }
    }

    private async Task ReceiveMessagesAsync(CancellationToken cancellationToken)
    {
        await foreach (string topicId in _topicsSource.ReadAllAsync(cancellationToken))
        {
            _ = Task.Run(async () =>
            {
                TopicName topicName = TopicName.FromProjectTopic(_config.ProjectId, topicId);
                SubscriptionName subscriptionName = SubscriptionName.FromProjectSubscription(_config.ProjectId, topicId + "-sub");
                SubscriberClient subscriberClient = await SubscriberClient.CreateAsync(subscriptionName);
                SubscriberServiceApiClient adminClient = await SubscriberServiceApiClient.CreateAsync(cancellationToken);

                try
                {
                    _ = await adminClient.CreateSubscriptionAsync(new()
                    {
                        TopicAsTopicName = topicName,
                        SubscriptionName = subscriptionName
                    });
                }
                catch (RpcException ex)
                {
                    _logger.LogWarning(ex, "Failed to create subscription {Subscription}. It probably already existed.", subscriptionName);
                }
                _ = cancellationToken.Register(() =>
                {
                    subscriberClient.StopAsync(CancellationToken.None).GetAwaiter().GetResult();
                });
                await subscriberClient.StartAsync(async (msg, ct) =>
                {
                    TransceiverMessage message = new(msg.Data.ToByteArray());
                    await _messageProcessor.ProcessMessageAsync(message, ct);
                    return SubscriberClient.Reply.Ack;
                });
            }, cancellationToken);
        }
    }
}