// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

namespace Transceiver.Demo;

public abstract class Example<TRequest, TResponse>
{
    private readonly bool _enableClient;
    private readonly ITransceiver<TRequest, TResponse> _transceiver;

    protected Example(ITransceiver<TRequest, TResponse> transceiver, bool enableClient = true)
    {
        _enableClient = enableClient;
        _transceiver = transceiver;
    }

    public abstract TRequest CreateRequest();

    public async Task Execute(CancellationToken cancellationToken)
    {
        if (!_enableClient)
        {
            return;
        }
        while (!cancellationToken.IsCancellationRequested)
        {
            TRequest request = CreateRequest();
            TResponse response = await _transceiver.TransceiveOnceAsync(request, cancellationToken);
            ProcessResponse(request, response);
        }
    }

    public abstract Task<TResponse> ProcessRequest(TRequest request, CancellationToken cancellationToken);

    public abstract void ProcessResponse(TRequest request, TResponse response);
}