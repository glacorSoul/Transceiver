// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

namespace Transceiver;

public class DirectProtocol : ITransceiverProtocol
{
    public DirectProtocol()
    {
    }

    public IAsyncEnumerable<T> ReceiveObjectsAsync<T>(Guid requestId, CancellationToken cancellationToken) where T : IIdentifiable
    {
        return null!;
    }

    public Task SendObjectToClientAsync<T>(T data, CancellationToken cancellationToken) where T : IIdentifiable
    {
        return Task.CompletedTask;
    }

    public Task SendObjectToServerAsync<T>(T data, CancellationToken cancellationToken) where T : IIdentifiable
    {
        return Task.CompletedTask;
    }
}