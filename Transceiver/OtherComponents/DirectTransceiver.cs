// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using System.Runtime.CompilerServices;

namespace Transceiver;

public sealed class DirectTransceiver<TRequest, TResponse> : ITransceiver<TRequest, TResponse>
{
    private readonly IPipelineProcessor<TRequest, TResponse> _pipeline;
    private readonly IProcessor<TRequest, TResponse> _processor;

    public DirectTransceiver(IProcessor<TRequest, TResponse> processor, IPipelineProcessor<TRequest, TResponse> pipeline)
    {
        _processor = processor;
        _pipeline = pipeline;
    }

    public async Task<ClientRequest<TRequest, TResponse>> SendToServerAsync(TRequest request, CancellationToken cancellationToken)
    {
        ClientRequest<TRequest, TResponse> clientRequest = new(request);
        Task<TResponse> Process(CancellationToken token)
        {
            return _processor.ProcessRequestAsync(clientRequest, token);
        }
        _ = await _pipeline.ProcessAsync(request, Process, cancellationToken);
        return clientRequest;
    }

    public Task StartProcessingRequestsAsync(IProcessor<TRequest, TResponse> processor, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async IAsyncEnumerable<TResponse> TransceiveManyAsync(TRequest request, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        Task<TResponse> Process(CancellationToken token)
        {
            ClientRequest<TRequest, TResponse> clientRequest = new(request);
            return _processor.ProcessRequestAsync(clientRequest, token);
        }
        TResponse response = await _pipeline.ProcessAsync(request, Process, cancellationToken);
        yield return response;
    }

    public async Task<TResponse> TransceiveOnceAsync(TRequest request, CancellationToken cancellationToken)
    {
        Task<TResponse> Process(CancellationToken token)
        {
            ClientRequest<TRequest, TResponse> clientRequest = new(request);
            return _processor.ProcessRequestAsync(clientRequest, token);
        }
        TResponse response = await _pipeline.ProcessAsync(request, Process, cancellationToken);
        return response;
    }
}