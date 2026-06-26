// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using System.Diagnostics;
using System.Threading.RateLimiting;
using Transceiver.Demo.Requests;

namespace Transceiver.Demo.Examples;

public class FiboExample : IDisposable
{
    private readonly RateLimiter _logLimiter = new FixedWindowRateLimiter(
        new FixedWindowRateLimiterOptions
        {
            PermitLimit = 1,
            Window = TimeSpan.FromSeconds(5),
            QueueLimit = 0,
            AutoReplenishment = true
        });

    private readonly ITransceiver<FiboRequest, FiboResponse> _transceiver;
    private bool disposedValue;

    public FiboExample(ITransceiver<FiboRequest, FiboResponse> transceiver)
    {
        _transceiver = transceiver;
    }

    public async Task Execute(CancellationToken cancellationToken)
    {
        long now = Stopwatch.GetTimestamp();
        int n = 0;
        while (!cancellationToken.IsCancellationRequested)
        {
            FiboRequest request = new() { N = n };
            IAsyncEnumerable<FiboResponse> response = _transceiver.TransceiveManyAsync(request, cancellationToken);
            await foreach (FiboResponse r in response)
            {
                RateLimitLease lease = await _logLimiter.AcquireAsync(1, cancellationToken);
                ++n;
                if (lease.IsAcquired)
                {
                    TimeSpan elapsed = TimeSpan.FromTicks(Stopwatch.GetTimestamp() - now);
                    Console.WriteLine($"Requests per second: {n / elapsed.TotalSeconds}");
                    Console.WriteLine("Fibonacci({0}) = {1}", n, r.Result);
                }
            }
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _logLimiter.Dispose();
            }

            disposedValue = true;
        }
    }

    ~FiboExample()
    {
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
