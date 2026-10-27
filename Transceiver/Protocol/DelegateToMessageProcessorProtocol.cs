// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using System.Runtime.CompilerServices;

namespace Transceiver;

public class DelegateToMessageProcessorProtocol : ITransceiverProtocol
{
    private readonly IMessageProcessor _messageProcessor;
    private readonly ISerializer _serializer;

    public DelegateToMessageProcessorProtocol(IMessageProcessor messageProcessor, ISerializer serializer)
    {
        _messageProcessor = messageProcessor;
        _serializer = serializer;
    }

    public async IAsyncEnumerable<T> ReceiveObjectsAsync<T>(Guid requestId, [EnumeratorCancellation] CancellationToken cancellationToken) where T : IIdentifiable
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
            _messageProcessor.RemoveRequester(requestId, source);
        }
    }

    public Task SendObjectToClientAsync<T>(T data, CancellationToken cancellationToken) where T : IIdentifiable
    {
        return SendObjectToServerAsync(data, cancellationToken);
    }

    public Task SendObjectToServerAsync<T>(T data, CancellationToken cancellationToken) where T : IIdentifiable
    {
        return _messageProcessor.ProcessMessageAsync(new TransceiverMessage(data, _serializer), cancellationToken);
    }
}