// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using System.Runtime.CompilerServices;

namespace Transceiver;

public class DirectProtocol : ITransceiverProtocol
{
    public DirectProtocol()
    {
    }

    public async IAsyncEnumerable<T> ReceiveObjectsAsync<T>(Guid requestId, [EnumeratorCancellation]CancellationToken cancellationToken) where T : IIdentifiable
    {
        await Task.CompletedTask;
        yield break;
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