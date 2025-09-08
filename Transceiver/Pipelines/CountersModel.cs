// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Reflection;

namespace Transceiver;

internal sealed class CountersModel<TRequest, TResponse>
{
    public CountersModel()
    {
        AssemblyName transceiverDll = typeof(MetricsProcessor<,>).Assembly.GetName();
        Meter = new(transceiverDll.Name!, transceiverDll.Version?.ToString() ?? "0.1.0");
        string metricName = typeof(TRequest).ToShortPrettyString() + typeof(TResponse).ToShortPrettyString();
        NErrorCounter = Meter.CreateCounter<long>($"requests_error_total_{metricName}",
            "errors",
            "Total number of requests that resulted in an error"
        );
        NRequestsCounter = Meter.CreateCounter<long>($"requests_total_{metricName}",
            "requests",
            "Total number of requests processed"
        );
        NSlowRequestsCounter = Meter.CreateCounter<long>($"requests_slow_total_{metricName}",
            "requests",
            $"Total number of requests that exceeded the execution SLA {ExecutionSla.TotalMilliseconds} ms"
        );
        ExecutionTimeCounter = Meter.CreateHistogram<double>($"request_execution_time_ms_{metricName}",
            "ms",
            "Execution time of requests in milliseconds"
        );
        RequestsPerSecond = Meter.CreateHistogram<double>($"requests_per_second_{metricName}",
            "rps",
            "Number of requests processed per second"
        );
        NRequestsListener = new();
        NRequestsListener.EnableMeasurementEvents(NRequestsCounter);
        long now = Stopwatch.GetTimestamp();
        NRequestsListener.SetMeasurementEventCallback<long>((instrument, measurment, tags, state) =>
        {
            TimeSpan elapsed = TimeSpan.FromTicks(Stopwatch.GetTimestamp() - now);
            if (measurment >= 1)
            {
                double requestsPerSecond = elapsed.TotalSeconds / measurment;
                RequestsPerSecond.Record(requestsPerSecond);
            }
        });
    }

    public TimeSpan ExecutionSla { get; } = TimeSpan.FromMilliseconds(500);
    public Histogram<double> ExecutionTimeCounter { get; }
    public Counter<long> NErrorCounter { get; }
    public Counter<long> NRequestsCounter { get; }
    public MeterListener NRequestsListener { get; }
    public Counter<long> NSlowRequestsCounter { get; }
    public Histogram<double> RequestsPerSecond { get; }
    private Meter Meter { get; }
}