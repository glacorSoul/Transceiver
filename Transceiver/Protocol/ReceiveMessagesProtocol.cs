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
#if NET8_0_OR_GREATER
    private readonly ConcurrentBag<CancellationTokenSource> _timeoutCtsPool = [];
#endif

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
    protected ISerializer Serializer => _serializer;

    public abstract Task<TTransceiver> SetupWriterAsync(CancellationToken cancellationToken);

    protected abstract Task<(int, object)> ReadAsync(TTransceiver reader, byte[] buffer, CancellationToken cancellationToken);

    protected abstract Task<TTransceiver> SetupReadAsync(CancellationToken cancellationToken);

    protected abstract Task WriteAsync(TTransceiver transceiver, object client, byte[] data, CancellationToken cancellationToken);

    public async Task ServerReceiveMessagesAsync(TTransceiver reader, CancellationToken cancellationToken)
    {
        PartitionedMessage partitionedMessage = new();
        while (!cancellationToken.IsCancellationRequested)
        {
            byte[] buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(Environment.SystemPageSize);
            try
            {
                (int read, _) = await ReadAsync(reader, buffer, cancellationToken);

                if (read == 0)
                {
                    break;
                }

                partitionedMessage.ParseBuffer(buffer, read);
                while (partitionedMessage.IsCompleted)
                {
                    await _messageProcessor.ProcessMessageAsync(partitionedMessage.Message!, cancellationToken);
                    partitionedMessage.ReturnPayloadBuffer();
                    ArraySegment<byte> remainingBytes = partitionedMessage.RemainingBytes;
                    partitionedMessage.Reset();
                    if (remainingBytes.Count > 0)
                    {
                        partitionedMessage.ParseBuffer(remainingBytes.Array!, remainingBytes.Count);
                        System.Buffers.ArrayPool<byte>.Shared.Return(remainingBytes.Array!);
                    }
                }
            }
            catch (OperationCanceledException ex)
            {
                Logger?.LogWarning(ex, "Receiving message was cancelled: {Message}", ex.Message);
            }
            finally
            {
                System.Buffers.ArrayPool<byte>.Shared.Return(buffer);
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

    public async IAsyncEnumerable<T> ReceiveObjectsAsync<T>(Guid requestId, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken) where T : IIdentifiable
    {
        IAsyncSource<T> source = _messageProcessor.AddRequester<T>(requestId);
        try
        {
            await foreach (T item in source.ReadAllAsync(cancellationToken))
            {
                yield return item;
            }
        }
        finally
        {
            _ = _clientsMap.TryRemove(requestId, out _);
            _messageProcessor.RemoveRequester(requestId, source);
        }
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
#if NET8_0_OR_GREATER
        if (!_timeoutCtsPool.TryTake(out CancellationTokenSource? cancelSource))
        {
            cancelSource = new CancellationTokenSource();
        }
        CancellationTokenRegistration registration = cancellationToken.CanBeCanceled
            ? cancellationToken.Register(static state => ((CancellationTokenSource)state!).Cancel(), cancelSource)
            : default;
        try
        {
            cancelSource.CancelAfter(Configuration.Value.RequestTimeout);

            TTransceiver writer = await SetupWriterAsync(cancelSource.Token);
            byte[] buffer = ToDataArray(data);
            await WriteAsync(writer, null!, buffer, cancelSource.Token);
        }
        finally
        {
            await registration.DisposeAsync();
            if (cancelSource.TryReset())
            {
                _timeoutCtsPool.Add(cancelSource);
            }
            else
            {
                cancelSource.Dispose();
            }
        }
#else
        using CancellationTokenSource cancelSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cancelSource.CancelAfter(Configuration.Value.RequestTimeout);
        cancellationToken = cancelSource.Token;

        TTransceiver writer = await SetupWriterAsync(cancellationToken);
        byte[] buffer = ToDataArray(data);
        await WriteAsync(writer, null!, buffer, cancellationToken);
#endif
    }

    protected virtual byte[] ToDataArray<T>(T data) where T : IIdentifiable
    {
        TransceiverMessage message = new(data, _serializer);
        return message.ToBytes();
    }

    private async Task ProcessIncomingMessages(TTransceiver reader, CancellationToken cancellationToken)
    {
        PartitionedMessage partitionedMessage = new();
        while (!cancellationToken.IsCancellationRequested)
        {
            byte[] buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(Environment.SystemPageSize);
            try
            {
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
                    partitionedMessage.ReturnPayloadBuffer();
                    ArraySegment<byte> remainingBytes = partitionedMessage.RemainingBytes;
                    partitionedMessage.Reset();
                    if (remainingBytes.Count > 0)
                    {
                        partitionedMessage.ParseBuffer(remainingBytes.Array!, remainingBytes.Count);
                        System.Buffers.ArrayPool<byte>.Shared.Return(remainingBytes.Array!);
                    }
                }
            }
            catch (OperationCanceledException ex)
            {
                Logger?.LogWarning(ex, "Receiving message was cancelled: {Message}", ex.Message);
            }
            finally
            {
                System.Buffers.ArrayPool<byte>.Shared.Return(buffer);
            }
        }
    }

    private async Task ReceiveMessage(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            TTransceiver reader = await SetupReadAsync(cancellationToken);
            _ = ProcessIncomingMessages(reader, cancellationToken);
        }
    }
}