// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Transceiver;

public interface IServerResponse<TResponse> : IIdentifiable
{
    DateTimeOffset TimeStamp { get; set; }
    TResponse Data { get; set; }
}

public class ServerResponse<TRequest, TResponse> : IServerResponse<TResponse>
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    [JsonConstructor]
    public ServerResponse()
    {
        Data = default!;
    }

    public ServerResponse(TResponse data, IClientRequest<TRequest, TResponse> request)
    {
        Id = request.Id;
        TimeStamp = DateTimeOffset.UtcNow;
        Data = data;
    }

    public TResponse Data { get; set; }
    public Guid Id { get; set; }
    public DateTimeOffset TimeStamp { get; set; }
}