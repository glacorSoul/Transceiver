// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace Transceiver;

public abstract class ReceiveMessagesProtocol<TTransceiver> : ITransceiverProtocol
{
    private readonly ConcurrentDictionary<Guid, (TTransceiver, object)> _clientsMap;
    private readonly IMessageProcessor _messageProcessor;
    private readonly ISerializer _serializer;

    protected ReceiveMessagesProtocol(
        IMessageProcessor messageProcessor,
        ISerializer serializer,
        ILogger logger,
        IOptions<TransceiverConfiguration> configuration)
    {
        _clientsMap = [];
        _serializer = serializer;
        _messageProcessor = messageProcessor;
        Logger = logger;
        Configuration = configuration;
    }

    protected IOptions<TransceiverConfiguration> Configuration { get; }
    protected ILogger Logger { get; }

    public async Task ReceiveMessagesAsync(TTransceiver reader, CancellationToken cancellationToken)
    {
        PartitionedMessage partitionedMessage = new();
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                byte[] buffer = new byte[Environment.SystemPageSize];
                (int read, _) = await ReadAsync(reader, buffer, cancellationToken);

                if (read == 0)
                {
                    break;
                }

                partitionedMessage.ParseBuffer(buffer, read);
                while (partitionedMessage.IsCompleted)
                {
                    await _messageProcessor.ProcessMessageAsync(partitionedMessage.Message!, cancellationToken);
                    PartitionedMessage oldMessage = partitionedMessage;
                    partitionedMessage = new PartitionedMessage();
                    if (oldMessage.RemainingBytes.Count > 0)
                    {
                        partitionedMessage.ParseBuffer([.. oldMessage.RemainingBytes], oldMessage.RemainingBytes.Count);
                    }
                }
            }
            catch (OperationCanceledException ex)
            {
                Logger?.LogWarning(ex, "Receiving message was cancelled: {Message}", ex.Message);
            }
        }
    }

    public async Task ReceiveMessagesAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await ReceiveMessage(cancellationToken);
        }
    }

    public AsyncSource<T> ReceiveObjects<T>(Guid requestId) where T : IIdentifiable
    {
        return _messageProcessor.AddRequester<T>(requestId);
    }

    public Task SendObjectToClientAsync<T>(T data, CancellationToken cancellationToken) where T : IIdentifiable
    {
        if (!_clientsMap.TryGetValue(data.Id, out (TTransceiver, object) client))
        {
            return Task.CompletedTask;
        }
        byte[] buffer = ToDataArray(data);
        return WriteAsync(client.Item1, client.Item2, buffer, cancellationToken);
    }

    public async Task SendObjectToServerAsync<T>(T data, CancellationToken cancellationToken) where T : IIdentifiable
    {
        using CancellationTokenSource cancelSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cancelSource.CancelAfter(Configuration.Value.RequestTimeout);
        cancellationToken = cancelSource.Token;

        TTransceiver writer = await SetupWriterAsync(cancellationToken);
        byte[] buffer = ToDataArray(data);
        await WriteAsync(writer, null!, buffer, cancellationToken);
    }

    public abstract Task<TTransceiver> SetupWriterAsync(CancellationToken cancellationToken);

    protected abstract Task<(int, object)> ReadAsync(TTransceiver reader, byte[] buffer, CancellationToken cancellationToken);

    protected abstract Task<TTransceiver> SetupReadAsync(CancellationToken cancellationToken);

    protected byte[] ToDataArray<T>(T data) where T : IIdentifiable
    {
        TransceiverMessage message = new(data, _serializer);
        return message.ToBytes();
    }

    protected abstract Task WriteAsync(TTransceiver transceiver, object client, byte[] data, CancellationToken cancellationToken);

    private async Task ReceiveMessage(CancellationToken cancellationToken)
    {
        TTransceiver reader = await SetupReadAsync(cancellationToken);

        PartitionedMessage partitionedMessage = new();
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                byte[] buffer = new byte[Environment.SystemPageSize];
                (int read, object client) = await ReadAsync(reader, buffer, cancellationToken);

                if (read == 0)
                {
                    break;
                }

                partitionedMessage.ParseBuffer(buffer, read);
                while (partitionedMessage.IsCompleted)
                {
                    _ = _clientsMap.AddOrUpdate(partitionedMessage.Header!.Id, _ => (reader, client), (_, _) => (reader, client));
                    await _messageProcessor.ProcessMessageAsync(partitionedMessage.Message!, cancellationToken);
                    PartitionedMessage oldMessage = partitionedMessage;
                    partitionedMessage = new PartitionedMessage();
                    if (oldMessage.RemainingBytes.Count > 0)
                    {
                        partitionedMessage.ParseBuffer([.. oldMessage.RemainingBytes], oldMessage.RemainingBytes.Count);
                    }
                }
            }
            catch (OperationCanceledException ex)
            {
                Logger?.LogWarning(ex, "Receiving message was cancelled: {Message}", ex.Message);
            }
        }
    }
}