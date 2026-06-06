// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Transceiver;

internal static class ClientRequest
{
    internal static readonly ITransceiverProtocol Protocol = BootStrap.ServiceProvider.GetRequiredService<ITransceiverProtocol>();
}

public class ClientRequest<TRequest, TResponse> : IIdentifiable
{


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

    public TRequest Data { get; set; }
    public Guid Id { get; set; }
    public DateTimeOffset TimeStamp { get; set; }

    public Task SendResponseAsync(TResponse response, CancellationToken cancellationToken)
    {
        if(ClientRequest.Protocol is DirectProtocol)
        {
            return Task.CompletedTask;
        }
        ServerResponse<TRequest, TResponse> serverResponse = new(response, this);
        return ClientRequest.Protocol.SendObjectToClientAsync(serverResponse, cancellationToken);
    }
}