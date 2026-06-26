// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Transceiver;

public interface IClientRequest<TRequest, in TResponse> : IIdentifiable
{
    DateTimeOffset TimeStamp { get; set; }
    TRequest Data { get; set; }
    Task SendResponseAsync(TResponse response, CancellationToken cancellationToken);
}

public class ClientRequest<TRequest, TResponse> : IClientRequest<TRequest, TResponse>
{

    [EditorBrowsable(EditorBrowsableState.Never)]
    public ClientRequest() : this(default!)
    {

    }

    [JsonConstructor]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public ClientRequest(TRequest data, Guid id, DateTimeOffset timeStamp)
    {
        Id = id;
        TimeStamp = timeStamp;
        Data = data;
    }

    public ClientRequest(TRequest data) : this(data, Guid.NewGuid(), DateTimeOffset.UtcNow)
    {
    }

    public virtual TRequest Data { get; set; }
    public virtual Guid Id { get; set; }
    public virtual DateTimeOffset TimeStamp { get; set; }

    public Task SendResponseAsync(TResponse response, CancellationToken cancellationToken)
    {
        if (Constants.Protocol is DirectProtocol)
        {
            return Task.CompletedTask;
        }
        IServerResponse<TResponse> serverResponse = Constants.RequestResponseFactory.CreateServerResponse(response, this);
        return Constants.Protocol.SendObjectToClientAsync(serverResponse, cancellationToken);
    }
}