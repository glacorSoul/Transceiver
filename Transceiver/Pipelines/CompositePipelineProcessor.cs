// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

namespace Transceiver;

public class CompositePipelineProcessor<TRequest, TResponse> : IPipelineProcessor<TRequest, TResponse>
{
    private readonly IEnumerable<IPipelineProcessor<TRequest, TResponse>> _processors;

    public CompositePipelineProcessor(IEnumerable<IPipelineProcessor<TRequest, TResponse>> processors)
    {
        _processors = processors;
    }

    public Task<TResponse> Process(TRequest request, Func<CancellationToken, Task<TResponse>> nextStep, CancellationToken cancellationToken)
    {
        Task<TResponse> Handle(CancellationToken token)
        {
            return nextStep(cancellationToken);
        }

        Func<CancellationToken, Task<TResponse>> nextHandle = Handle;
        foreach (IPipelineProcessor<TRequest, TResponse> processor in _processors.Reverse())
        {
            nextHandle = (token) => processor.Process(request, nextHandle, token);
        }
        return nextHandle(cancellationToken);
    }
}