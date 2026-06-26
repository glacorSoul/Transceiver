// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using System.Runtime.CompilerServices;

namespace Transceiver;

public sealed class Transceiver<TRequest, TResponse> : ITransceiver<TRequest, TResponse>
{
    private readonly IPipelineProcessor<TRequest, TResponse> _pipeline;
    private readonly ITransceiverProtocol _protocol;
    private readonly IRequestResponseFactory _requestResponseFactory;

    public Transceiver(ITransceiverProtocol protocol, IPipelineProcessor<TRequest, TResponse> pipeline, IRequestResponseFactory requestResponseFactory)
    {
        _protocol = protocol;
        _pipeline = pipeline;
        _requestResponseFactory = requestResponseFactory;
    }

    public async Task<IIdentifiable> SendToServerAsync(IIdentifiable request, CancellationToken cancellationToken)
    {
        await _protocol.SendObjectToServerAsync(request, cancellationToken);
        return request;
    }

    public async Task<IClientRequest<TRequest, TResponse>> SendToServerAsync(TRequest request, CancellationToken cancellationToken)
    {
        IIdentifiable clientRequest = _requestResponseFactory.CreateClientRequest<TRequest, TResponse>(request);
        IIdentifiable result = await SendToServerAsync(clientRequest, cancellationToken);
        return (IClientRequest<TRequest, TResponse>)result;
    }

    public async Task StartProcessingRequestsAsync(IProcessor<TRequest, TResponse> processor, CancellationToken cancellationToken)
    {
        IAsyncEnumerable<IClientRequest<TRequest, TResponse>> requests = _protocol
            .ReceiveObjectsAsync<IClientRequest<TRequest, TResponse>>(Guid.Empty, cancellationToken);
        await foreach (IClientRequest<TRequest, TResponse> request in requests)
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
        IIdentifiable clientRequest = _requestResponseFactory.CreateClientRequest<TRequest, TResponse>(request);
        IAsyncEnumerable<IServerResponse<TResponse>> responses = _protocol
            .ReceiveObjectsAsync<IServerResponse<TResponse>>(clientRequest.Id, cancellationToken);
        _ = await SendToServerAsync(clientRequest, cancellationToken);

        await foreach (IServerResponse<TResponse> serverResponse in responses)
        {
            yield return serverResponse.Data;
        }
    }

    public async Task<TResponse> TransceiveOnceAsync(TRequest request, CancellationToken cancellationToken)
    {
        IIdentifiable clientRequest = _requestResponseFactory.CreateClientRequest<TRequest, TResponse>(request);
        IAsyncEnumerable<IServerResponse<TResponse>> responses = _protocol
            .ReceiveObjectsAsync<IServerResponse<TResponse>>(clientRequest.Id, cancellationToken);
        _ = await SendToServerAsync(clientRequest, cancellationToken);

        IAsyncEnumerator<IServerResponse<TResponse>> enumerator = responses.GetAsyncEnumerator(cancellationToken);
        try
        {
            if (await enumerator.MoveNextAsync())
            {
                return enumerator.Current.Data;
            }
            return default!;
        }
        finally
        {
            await enumerator.DisposeAsync();
        }
    }
}