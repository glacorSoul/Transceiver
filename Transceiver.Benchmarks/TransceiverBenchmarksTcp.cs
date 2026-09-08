// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Reflection;

namespace Transceiver.Benchmarks;

[MemoryDiagnoser]
[RPlotExporter]
public class TransceiverBenchmarksTcp
{
    private ITransceiver<TcpSumRequest, TcpSumResponse> _tcpTransceiver = default!;

    private readonly TcpSumRequest _tcpSumRequest = new()
    {
        A = 1,
        B = 2
    };

    private static ITransceiver<TcpSumRequest, TcpSumResponse> BuildTcpTransceiver(CancellationToken cancellationToken)
    {
        ServiceCollection services = [];
        _ = services.AddLogging(builder => builder.AddConsole());
        _ = services.AddTransceiver(transceiverConfiguration =>
        {
            ITransceiverSetup setup = transceiverConfiguration.ConfigureTcp(new(IPAddress.Loopback, 1125));
            setup.SetupServer(false, cancellationToken);
            setup.SetupClient(cancellationToken);
        }, Assembly.GetExecutingAssembly());
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        return serviceProvider.GetRequiredService<ITransceiver<TcpSumRequest, TcpSumResponse>>();
    }

    [GlobalSetup]
    public void Setup()
    {
        _tcpTransceiver = BuildTcpTransceiver(CancellationToken.None);
    }

    [Benchmark]
    public Task<TcpSumResponse> TransceiverTCPSum()
    {
        return _tcpTransceiver.TransceiveOnceAsync(_tcpSumRequest, CancellationToken.None);
    }
}
