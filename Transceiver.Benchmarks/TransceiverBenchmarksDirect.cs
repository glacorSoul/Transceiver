// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Transceiver.Benchmarks;

[MemoryDiagnoser]
[RPlotExporter]
public class TransceiverBenchmarksDirect
{
    private ITransceiver<DirectSumRequest, DirectSumResponse> _directSumTransceiver = default!;
    private readonly DirectSumRequest _directSumRequest = new()
    {
        A = 1,
        B = 2
    };

    //------------------------------Transceiver benchmarks------------------------------
    private static ITransceiver<DirectSumRequest, DirectSumResponse> BuildDirectTransceiver()
    {
        ServiceCollection services = [];
        _ = services.AddLogging(builder => builder.AddConsole());
        _ = services.AddTransceiver(transceiverConfiguration => transceiverConfiguration.ConfigureDirectProtocol(), Assembly.GetExecutingAssembly());
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        return serviceProvider.GetRequiredService<ITransceiver<DirectSumRequest, DirectSumResponse>>();
    }

    [GlobalSetup]
    public void Setup()
    {
        _directSumTransceiver = BuildDirectTransceiver();
    }

    [Benchmark]
    public Task<DirectSumResponse> TransceiverDirectSum()
    {
        return _directSumTransceiver.TransceiveOnceAsync(_directSumRequest, CancellationToken.None);
    }

    [Benchmark]
    public Task<DirectSumResponse> TransceiverDirectSumWithReply()
    {
        return _directSumTransceiver.TransceiveOnceAsync(_directSumRequest, CancellationToken.None);
    }
}
