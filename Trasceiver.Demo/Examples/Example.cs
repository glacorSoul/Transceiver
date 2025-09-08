// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using System.Diagnostics;
using System.Threading.RateLimiting;

namespace Transceiver.Demo;

public abstract class Example<TRequest, TResponse>
{
    private readonly RateLimiter _logLimiter = new FixedWindowRateLimiter(
        new FixedWindowRateLimiterOptions
        {
            PermitLimit = 1,
            Window = TimeSpan.FromSeconds(5),
            QueueLimit = 0,
            AutoReplenishment = true
        });

    private readonly ITransceiver<TRequest, TResponse> _transceiver;

    protected Example(ITransceiver<TRequest, TResponse> transceiver)
    {
        _transceiver = transceiver;
    }

    public abstract TRequest CreateRequest();

    public async Task Execute(CancellationToken cancellationToken)
    {
        long now = Stopwatch.GetTimestamp();
        int n = 0;
        while (!cancellationToken.IsCancellationRequested)
        {
            TRequest request = CreateRequest();
            TResponse response = await _transceiver.TransceiveOnceAsync(request, cancellationToken);
            ++n;
            RateLimitLease lease = await _logLimiter.AcquireAsync(1, cancellationToken);
            if (lease.IsAcquired)
            {
                TimeSpan elapsed = TimeSpan.FromTicks(Stopwatch.GetTimestamp() - now);
                Console.WriteLine($"Requests per second: {n / elapsed.TotalSeconds}");
            }
            ProcessResponse(lease, request, response);
        }
    }

    public abstract Task<TResponse> ProcessRequest(TRequest request, CancellationToken cancellationToken);

    public abstract void ProcessResponse(RateLimitLease lease, TRequest request, TResponse response);
}