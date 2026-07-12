// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using BenchmarkDotNet.Attributes;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Reflection;

namespace Transceiver.Benchmarks;

[MemoryDiagnoser]
[RPlotExporter]
public class TransceiverBenchmarks
{
    private ITransceiver<DirectSumRequest, DirectSumResponse> _directSumTransceiver = default!;
    private ITransceiver<DirectSumRequest, DirectSumResponse> _directSumTransceiverWithoutMetrics = default!;
    private ITransceiver<SslSumRequest, SslSumResponse> _sslTransceiver = default!;
    private ITransceiver<TcpSumRequest, TcpSumResponse> _tcpTransceiver = default!;
    private IMediator _mediator = default!;
    private readonly DirectSumRequest _directSumRequest = new()
    {
        A = 1,
        B = 2
    };
    private readonly SslSumRequest _sslSumRequest = new()
    {
        A = 1,
        B = 2
    };
    private readonly TcpSumRequest _tcpSumRequest = new()
    {
        A = 1,
        B = 2
    };

    public TransceiverBenchmarks() { }

    //------------------------------Transceiver benchmarks------------------------------
    private static ITransceiver<DirectSumRequest, DirectSumResponse> BuildDirectTransceiver()
    {
        ServiceCollection services = [];
        _ = services.AddLogging(builder => builder.AddConsole());
        _ = services.AddTransceiver(t => t.ConfigureDirectProtocol(), Assembly.GetExecutingAssembly());
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        return serviceProvider.GetRequiredService<ITransceiver<DirectSumRequest, DirectSumResponse>>();
    }

    private static ITransceiver<DirectSumRequest, DirectSumResponse> BuildDirectTransceiverWithoutMetrics()
    {
        ServiceCollection services = [];
        _ = services.AddLogging(builder => builder.AddConsole());
        _ = services.AddTransceiver(t => t.ConfigureDirectProtocol(), Assembly.GetExecutingAssembly());
        _ = services.RemoveAll<IPipelineProcessor<DirectSumRequest, DirectSumResponse>>();
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        return serviceProvider.GetRequiredService<ITransceiver<DirectSumRequest, DirectSumResponse>>();
    }

    private static ITransceiver<SslSumRequest, SslSumResponse> BuildSslTransceiver()
    {
        ServiceCollection services = [];
        _ = services.AddLogging(builder => builder.AddConsole());
        _ = services.AddTransceiver(t =>
        {
            ITransceiverSetup setup = t.ConfigureSsl(new(IPAddress.Loopback, 1124));
            setup.SetupServer(false);
            setup.SetupClient();
        }, Assembly.GetExecutingAssembly());
        _ = services.Configure<TransceiverConfiguration>(cfg =>
        {
            cfg.CertificateThumbprint = "1c39b7a9e05f5c8124189a950a51779ecd1fdf93";
        });
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        return serviceProvider.GetRequiredService<ITransceiver<SslSumRequest, SslSumResponse>>();
    }

    private static ITransceiver<TcpSumRequest, TcpSumResponse> BuildTcpTransceiver()
    {
        ServiceCollection services = [];
        _ = services.AddLogging(builder => builder.AddConsole());
        _ = services.AddTransceiver(t => t.ConfigureTcp(new(IPAddress.Loopback, 1125)), Assembly.GetExecutingAssembly());
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        return serviceProvider.GetRequiredService<ITransceiver<TcpSumRequest, TcpSumResponse>>();
    }

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
        _directSumTransceiver = BuildDirectTransceiver();
        _directSumTransceiverWithoutMetrics = BuildDirectTransceiverWithoutMetrics();
        _sslTransceiver = BuildSslTransceiver();
        _tcpTransceiver = BuildTcpTransceiver();
        _mediator = BuildMediatR();
    }

    [Benchmark]
    public Task<DirectSumResponse> TransceiverDirectSum()
    {
        return _directSumTransceiver.TransceiveOnceAsync(_directSumRequest, CancellationToken.None);
    }

    [Benchmark]
    public Task<DirectSumResponse> TransceiverDirectSumWithoutMetrics()
    {
        return _directSumTransceiverWithoutMetrics.TransceiveOnceAsync(_directSumRequest, CancellationToken.None);
    }

    [Benchmark]
    public Task<DirectSumResponse> TransceiverDirectSumWithReply()
    {
        return _directSumTransceiver.TransceiveOnceAsync(_directSumRequest, CancellationToken.None);
    }

    [Benchmark]
    public Task<TcpSumResponse> TransceiverTCPSum()
    {
        return _tcpTransceiver.TransceiveOnceAsync(_tcpSumRequest, CancellationToken.None);
    }

    [Benchmark]
    public Task<SslSumResponse> TransceiverSslSum()
    {
        return _sslTransceiver.TransceiveOnceAsync(_sslSumRequest, CancellationToken.None);
    }

    //------------------------------MediatR benchmarks------------------------------
    [Benchmark]
    public Task<DirectSumResponse> MediatR()
    {
        return _mediator.Send(_directSumRequest);
    }
}
