// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Reflection;

namespace Transceiver;

public class CountersModel
{
    public static readonly ConcurrentBag<CountersModel> Instances = [];

    static CountersModel()
    {
        AssemblyName transceiverDll = typeof(MetricsPipelineProcessor<,>).Assembly.GetName();
        Meter = new(transceiverDll.Name!, transceiverDll.Version?.ToString() ?? "0.1.0");
    }

    public CountersModel()
    {
        Instances.Add(this);
    }

    public static Meter Meter { get; private set; }
    public Histogram<double> ErrorsPerSecond { get; protected set; } = default!;
    public TimeSpan ExecutionSla { get; } = TimeSpan.FromMilliseconds(500);
    public Histogram<double> ExecutionTimeCounter { get; protected set; } = default!;
    public string Name { get; protected set; } = default!;
    public Counter<long> NErrorsCounter { get; protected set; } = default!;
    public Counter<long> NRequestsCounter { get; protected set; } = default!;
    public Counter<long> NSlowRequestsCounter { get; protected set; } = default!;
    public Histogram<double> RequestsPerSecond { get; protected set; } = default!;
}

internal sealed class CountersModel<TRequest, TResponse> : CountersModel
{
    private readonly MeterListener _nErrorsListener;
    private readonly MeterListener _nRequestsListener;

    public CountersModel()
    {
        string metricName = typeof(TRequest).ToShortPrettyString() + typeof(TResponse).ToShortPrettyString();
        Name = metricName;
        NErrorsCounter = Meter.CreateCounter<long>($"requests_error_total_{metricName}",
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
        _nRequestsListener = new();
        _nRequestsListener.EnableMeasurementEvents(NRequestsCounter);
        long now = Stopwatch.GetTimestamp();
        _nRequestsListener.SetMeasurementEventCallback<long>((instrument, measurment, tags, state) =>
        {
            TimeSpan elapsed = TimeSpan.FromTicks(Stopwatch.GetTimestamp() - now);
            if (measurment >= 1)
            {
                double requestsPerSecond = measurment / elapsed.TotalSeconds;
                RequestsPerSecond.Record(requestsPerSecond, tags);
            }
        });
        _nErrorsListener = new();
        _nErrorsListener.EnableMeasurementEvents(NErrorsCounter);
        _nErrorsListener.SetMeasurementEventCallback<long>((instrument, measurment, tags, state) =>
        {
            TimeSpan elapsed = TimeSpan.FromTicks(Stopwatch.GetTimestamp() - now);
            if (measurment >= 1)
            {
                double requestsPerSecond = measurment / elapsed.TotalSeconds;
                ErrorsPerSecond.Record(requestsPerSecond, tags);
            }
        });
    }
}