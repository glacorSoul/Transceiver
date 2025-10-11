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

    public async Task<ClientRequest<TRequest, TResponse>> SendToServerAsync(TRequest request, CancellationToken cancellationToken)
    {
        ClientRequest<TRequest, TResponse> clientRequest = new(request);
        await _protocol.SendObjectToServerAsync(clientRequest, cancellationToken);
        return clientRequest;
    }

    public async Task StartProcessingRequestsAsync(IProcessor<TRequest, TResponse> processor, CancellationToken cancellationToken)
    {
        IAsyncEnumerable<ClientRequest<TRequest, TResponse>> requests = _protocol
            .ReceiveObjects<ClientRequest<TRequest, TResponse>>(Guid.Empty)
            .ReadAllAsync(cancellationToken);
        await foreach (ClientRequest<TRequest, TResponse> request in requests)
        {
            async Task<TResponse> Process(CancellationToken token)
            {
                TResponse response = await processor.ProcessRequest(request.Data, token);
                await request.SendResponseAsync(response, token);
                return response;
            }
            _ = await _pipeline.Process(request.Data, Process, cancellationToken);
        }
    }

    public async IAsyncEnumerable<TResponse> TransceiveMany(TRequest request, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        ClientRequest<TRequest, TResponse> clientRequest = await SendToServerAsync(request, cancellationToken);
        IAsyncEnumerable<ServerResponse<TRequest, TResponse>> responses = _protocol
            .ReceiveObjects<ServerResponse<TRequest, TResponse>>(clientRequest.Id)
            .ReadAllAsync(cancellationToken);
        await foreach (ServerResponse<TRequest, TResponse> serverResponse in responses)
        {
            yield return serverResponse.Data;
        }
    }

    public async Task<TResponse> TransceiveOnceAsync(TRequest request, CancellationToken cancellationToken)
    {
        ClientRequest<TRequest, TResponse> clientRequest = await SendToServerAsync(request, cancellationToken);
        IAsyncEnumerable<ServerResponse<TRequest, TResponse>> responses = _protocol
            .ReceiveObjects<ServerResponse<TRequest, TResponse>>(clientRequest.Id)
            .ReadAllAsync(cancellationToken);

        TResponse result = default!;
        await foreach (ServerResponse<TRequest, TResponse> serverResponse in responses)
        {
#pragma warning disable S1751 // Loops with at most one iteration should be refactored
            result = serverResponse.Data;
            break;
#pragma warning restore S1751 // Loops with at most one iteration should be refactored
        }
        return result;
    }
}