// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Transceiver.AmazonSqs;

public sealed class AmazonSqsMessageProcessor : IMessageProcessor, IDisposable
{
    private readonly CancellationTokenSource _cts = new();
    private readonly ILogger<AmazonSqsMessageProcessor> _logger;
    private readonly CorrelatedMessageProcessor _processor;
    private readonly ConcurrentDictionary<string, CreateQueueResponse> _queues = new();
    private readonly Thread _receiveMessages;
    private readonly ISerializer _serializer;
    private readonly Func<AmazonSQSClient> _sqsFactory;
    private bool _disposed;

    public AmazonSqsMessageProcessor(Func<AmazonSQSClient> sqs,
        CorrelatedMessageProcessor processor,
        ILogger<AmazonSqsMessageProcessor> logger,
        ISerializer serializer)
    {
        _sqsFactory = sqs;
        _processor = processor;
        _logger = logger;
        _receiveMessages = new(() => ProcessMessagesAsync(_cts.Token).GetAwaiter().GetResult());
        _receiveMessages.Start();
        _serializer = serializer;
    }

    ~AmazonSqsMessageProcessor()
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
        CreateQueueResponse queue = CreateOrGetQueue(message.Header.Type, cancellationToken);
        using AmazonSQSClient _sqs = _sqsFactory();
        _ = await _sqs.SendMessageAsync(new()
        {
            QueueUrl = queue.QueueUrl,
            MessageBody = message.ToCsv(),
        }, cancellationToken);
    }

    private CreateQueueResponse CreateOrGetQueue(Type type, CancellationToken cancellationToken)
    {
        string name = type.ToShortPrettyString().ToLettersOnly();
        return _queues.GetOrAdd(name, name =>
        {
            using AmazonSQSClient _sqs = _sqsFactory();
            CreateQueueResponse queue = _sqs.CreateQueueAsync(name, cancellationToken).GetAwaiter().GetResult();
            Thread.Sleep(TimeSpan.FromSeconds(2));
            return queue;
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
        }

        _disposed = true;
    }

    private async Task ProcessMessagesAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            IEnumerable<Task> tasks = _queues.Values
                .Select(async queue =>
                {
                    try
                    {
                        using AmazonSQSClient _sqs = _sqsFactory();
                        ReceiveMessageResponse response = await _sqs.ReceiveMessageAsync(new ReceiveMessageRequest
                        {
                            QueueUrl = queue.QueueUrl,
                            MaxNumberOfMessages = 10,
                            WaitTimeSeconds = 15,
                        }, cancellationToken);
                        foreach (Message queueMessage in response.Messages ?? [])
                        {
                            TransceiverMessage message = new(queueMessage.Body);
                            await _processor.ProcessMessageAsync(message, cancellationToken);
                            _ = await _sqs.DeleteMessageAsync(new DeleteMessageRequest
                            {
                                QueueUrl = queue.QueueUrl,
                                ReceiptHandle = queueMessage.ReceiptHandle
                            }, cancellationToken);
                        }
                    }
                    catch (QueueDoesNotExistException)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1));
                    }
                    catch (Exception ex) when (ex is IOException or HttpRequestException)
                    {
                        _logger.LogError(ex, "Error processing messages from queue {QueueUrl}", queue.QueueUrl);
                    }
                });
            await Task.WhenAll(tasks);
        }
    }
}