// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

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

    public AsyncSource<T> ReceiveObjects<T>(Guid requestId) where T : IIdentifiable
    {
        return _messageProcessor.AddRequester<T>(requestId);
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