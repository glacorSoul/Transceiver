// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using System.Runtime.CompilerServices;

namespace Transceiver;

public sealed class Transceiver<TRequest, TResponse> : ITransceiver<TRequest, TResponse>
{
    private readonly IPipelineProcessor<TRequest, TResponse> _pipeline;
    private readonly ITransceiverProtocol _protocol;

    public Transceiver(ITransceiverProtocol protocol, IPipelineProcessor<TRequest, TResponse> pipeline)
    {
        _protocol = protocol;
        _pipeline = pipeline;
    }

    public Task<ClientRequest<TRequest, TResponse>> SendToServerAsync(TRequest request, CancellationToken cancellationToken)
    {
        return SendToServerAsync(new ClientRequest<TRequest, TResponse>(request), cancellationToken);
    }

    public async Task<ClientRequest<TRequest, TResponse>> SendToServerAsync(ClientRequest<TRequest, TResponse> request, CancellationToken cancellationToken)
    {
        await _protocol.SendObjectToServerAsync(request, cancellationToken);
        return request;
    }

    public async Task StartProcessingRequestsAsync(IProcessor<TRequest, TResponse> processor, CancellationToken cancellationToken)
    {
        IAsyncEnumerable<ClientRequest<TRequest, TResponse>> requests = _protocol
            .ReceiveObjectsAsync<ClientRequest<TRequest, TResponse>>(Guid.Empty, cancellationToken);
        await foreach (ClientRequest<TRequest, TResponse> request in requests)
        {
            Task<TResponse> Process(CancellationToken token)
            {
                return processor.ProcessRequestAsync(request, token);
            }
            _ = await _pipeline.ProcessAsync(request.Data, Process, cancellationToken);
        }
    }

    public async IAsyncEnumerable<TResponse> TransceiveManyAsync(TRequest request, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        ClientRequest<TRequest, TResponse> clientRequest = new(request);
        IAsyncEnumerable<ServerResponse<TRequest, TResponse>> responses = _protocol
            .ReceiveObjectsAsync<ServerResponse<TRequest, TResponse>>(clientRequest.Id, cancellationToken);
        _ = await SendToServerAsync(clientRequest, cancellationToken);

        await foreach (ServerResponse<TRequest, TResponse> serverResponse in responses)
        {
            yield return serverResponse.Data;
        }
    }

    public async Task<TResponse> TransceiveOnceAsync(TRequest request, CancellationToken cancellationToken)
    {
        ClientRequest<TRequest, TResponse> clientRequest = new(request);
        IAsyncEnumerable<ServerResponse<TRequest, TResponse>> responses = _protocol
            .ReceiveObjectsAsync<ServerResponse<TRequest, TResponse>>(clientRequest.Id, cancellationToken);
        _ = await SendToServerAsync(clientRequest, cancellationToken);

        IAsyncEnumerator<ServerResponse<TRequest, TResponse>> enumerator = responses.GetAsyncEnumerator(cancellationToken);
        try
        {
            _ = await enumerator.MoveNextAsync();
            return enumerator.Current.Data;
        }
        finally
        {
            await enumerator.DisposeAsync();
        }
    }
}