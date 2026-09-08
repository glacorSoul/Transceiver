// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Transceiver.Benchmarks;

[MemoryDiagnoser]
[RPlotExporter]
public class TransceiverBenchmarksDirectWithoutMetrics
{
    private ITransceiver<DirectSumRequest, DirectSumResponse> _directSumTransceiverWithoutMetrics = default!;
    private readonly DirectSumRequest _directSumRequest = new()
    {
        A = 1,
        B = 2
    };

    private static ITransceiver<DirectSumRequest, DirectSumResponse> BuildDirectTransceiverWithoutMetrics()
    {
        ServiceCollection services = [];
        _ = services.AddLogging(builder => builder.AddConsole());
        _ = services.AddTransceiver(transceiverConfiguration => transceiverConfiguration.ConfigureDirectProtocol(), Assembly.GetExecutingAssembly());
        _ = services.RemoveAll<IPipelineProcessor<DirectSumRequest, DirectSumResponse>>();
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        return serviceProvider.GetRequiredService<ITransceiver<DirectSumRequest, DirectSumResponse>>();
    }

    [GlobalSetup]
    public void Setup()
    {
        _directSumTransceiverWithoutMetrics = BuildDirectTransceiverWithoutMetrics();
    }

    [Benchmark]
    public Task<DirectSumResponse> TransceiverDirectSumWithoutMetrics()
    {
        return _directSumTransceiverWithoutMetrics.TransceiveOnceAsync(_directSumRequest, CancellationToken.None);
    }
}
