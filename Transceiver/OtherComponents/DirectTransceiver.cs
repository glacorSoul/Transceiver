// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

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
        async Task<TResponse> Process(CancellationToken token)
        {
            TResponse response = await _processor.ProcessRequestAsync(request, token);
            return response;
        }
        _ = await _pipeline.ProcessAsync(request, Process, cancellationToken);
        return new ClientRequest<TRequest, TResponse>(request);
    }

    public Task StartProcessingRequestsAsync(IProcessor<TRequest, TResponse> processor, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<TResponse> TransceiveManyAsync(TRequest request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task<TResponse> TransceiveOnceAsync(TRequest request, CancellationToken cancellationToken)
    {
        async Task<TResponse> Process(CancellationToken token)
        {
            TResponse result = await _processor.ProcessRequestAsync(request, token);
            return result;
        }
        TResponse response = await _pipeline.ProcessAsync(request, Process, cancellationToken);
        return response;
    }
}