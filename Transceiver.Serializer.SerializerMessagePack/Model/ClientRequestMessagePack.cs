// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using MessagePack;

namespace Transceiver.Serializer.SerializerMessagePack;

[MessagePackObject]
public class ClientRequestMessagePack<TRequest, TResponse> : IClientRequest<TRequest, TResponse>
{
    private readonly ClientRequest<TRequest, TResponse> _clientRequest;
    public ClientRequestMessagePack() : this(default!)
    {
    }

    public ClientRequestMessagePack(TRequest data) : this(data, Guid.NewGuid(), DateTimeOffset.UtcNow)
    {
        _clientRequest = new ClientRequest<TRequest, TResponse>(data);
    }

    public ClientRequestMessagePack(TRequest data, Guid id, DateTimeOffset timeStamp)
    {
        _clientRequest = new ClientRequest<TRequest, TResponse>(data, id, timeStamp);
        Data = data;
        Id = id;
        TimeStamp = timeStamp;
    }

    [Key(0)]
    public DateTimeOffset TimeStamp { get; set; }
    [Key(1)]
    public TRequest Data { get; set; }
    [Key(2)]
    public Guid Id { get; set; }

    public Task SendResponseAsync(TResponse response, CancellationToken cancellationToken)
    {
        _clientRequest.Id = Id;
        return _clientRequest.SendResponseAsync(response, cancellationToken);
    }
}
