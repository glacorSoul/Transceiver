// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using BenchmarkDotNet.Attributes;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Transceiver.Benchmarks;

[MemoryDiagnoser]
[RPlotExporter]
public class TransceiverBenchmarksMediatR
{
    private IMediator _mediator = default!;
    private readonly DirectSumRequest _directSumRequest = new()
    {
        A = 1,
        B = 2
    };

    private static IMediator BuildMediatR()
    {
        ServiceCollection services = [];
        _ = services.AddLogging(builder => builder.AddConsole());
        _ = services.AddMediatR(Assembly.GetExecutingAssembly());
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        return serviceProvider.GetRequiredService<IMediator>();
    }

    [GlobalSetup]
    public void Setup()
    {
        _mediator = BuildMediatR();
    }

    [Benchmark]
    public Task<DirectSumResponse> MediatR()
    {
        return _mediator.Send(_directSumRequest);
    }
}
