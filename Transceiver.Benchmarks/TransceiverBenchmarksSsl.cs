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
public class TransceiverBenchmarksSsl
{
    private ITransceiver<SslSumRequest, SslSumResponse> _sslTransceiver = default!;
    private readonly SslSumRequest _sslSumRequest = new()
    {
        A = 1,
        B = 2
    };

    private static ITransceiver<SslSumRequest, SslSumResponse> BuildSslTransceiver(CancellationToken cancellationToken)
    {
        ServiceCollection services = [];
        _ = services.AddLogging(builder => builder.AddConsole());
        _ = services.AddTransceiver(transceiverConfiguration =>
        {
            ITransceiverSetup setup = transceiverConfiguration.ConfigureSsl(new(IPAddress.Loopback, 1124));
            setup.SetupServer(false, cancellationToken);
            setup.SetupClient(cancellationToken);
        }, Assembly.GetExecutingAssembly());
        _ = services.Configure<TransceiverConfiguration>(cfg =>
        {
            cfg.CertificateThumbprint = "34abfd4fc9e26a3315e1c398f51ebfc41ba0d553";
        });
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        return serviceProvider.GetRequiredService<ITransceiver<SslSumRequest, SslSumResponse>>();
    }

    [GlobalSetup]
    public void Setup()
    {
        _sslTransceiver = BuildSslTransceiver(CancellationToken.None);
    }

    [Benchmark]
    public Task<SslSumResponse> TransceiverSslSum()
    {
        return _sslTransceiver.TransceiveOnceAsync(_sslSumRequest, CancellationToken.None);
    }
}
