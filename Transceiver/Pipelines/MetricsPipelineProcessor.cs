// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.RateLimiting;

namespace Transceiver;

internal class MetricsPipelineProcessor<TRequest, TResponse> : IPipelineProcessor<TRequest, TResponse>
{
    private static readonly RateLimiter _logLimiter = new FixedWindowRateLimiter(
        new FixedWindowRateLimiterOptions
        {
            PermitLimit = 1,
            Window = TimeSpan.FromSeconds(5),
            QueueLimit = 0,
            AutoReplenishment = true
        });

    private static readonly CountersModel<TRequest, TResponse> Counters = new();
    private readonly ILogger<MetricsPipelineProcessor<TRequest, TResponse>> _logger;

    public MetricsPipelineProcessor(ILogger<MetricsPipelineProcessor<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Process(TRequest request, Func<CancellationToken, Task<TResponse>> nextStep, CancellationToken cancellationToken)
    {
        long now = Stopwatch.GetTimestamp();
        try
        {
            Counters.NRequestsCounter.Add(1);
            return await nextStep(cancellationToken);
        }
        catch (Exception ex)
        {
            Counters.NErrorsCounter.Add(1, new("request", request), new("error", ex));
            throw;
        }
        finally
        {
            TimeSpan elapsed = TimeSpan.FromTicks(Stopwatch.GetTimestamp() - now);
            RateLimitLease lease = await _logLimiter.AcquireAsync(1, cancellationToken);
            if (lease.IsAcquired)
            {
                _logger.LogInformation("Processed request of type {RequestType} in {ExecutionTime}", typeof(TRequest), elapsed);
            }
            if (elapsed > Counters.ExecutionSla)
            {
                Counters.NSlowRequestsCounter.Add(1, new("request", request), new("elapsed", elapsed));
            }
            Counters.ExecutionTimeCounter.Record(elapsed.TotalMilliseconds, new KeyValuePair<string, object?>("request", request));
        }
    }
}