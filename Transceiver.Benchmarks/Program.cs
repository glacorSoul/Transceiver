// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using System.Diagnostics;
using System.Net;
using System.Reflection;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Transceiver.Benchmarks;

internal class Program
{
#pragma warning disable IDE0052 // Remove unread private members
#pragma warning disable S4487 // Unread "private" fields should be removed
#pragma warning disable RCS1213 // Remove unused member declaration
#pragma warning disable S1144 // Unused private types or members should be removed
#pragma warning disable IDE0051 // Remove unused private members
    private readonly ITransceiver<DirectSumRequest, DirectSumResponse> _directSumTransceiver = default!;
    private readonly ITransceiver<DirectSumRequest, DirectSumResponse> _directSumTransceiverWithoutMetrics = default!;
    private readonly ITransceiver<DirectSumRequestWithReply, DirectSumResponseWithReply> _directSumTransceiverWithReply = default!;
    private readonly ITransceiver<SslSumRequest, SslSumResponse> _sslTransceiver = default!;
    private readonly ITransceiver<TcpSumRequest, TcpSumResponse> _tcpTransceiver = default!;
#pragma warning restore IDE0051 // Remove unused private members
#pragma warning restore S1144 // Unused private types or members should be removed
#pragma warning restore RCS1213 // Remove unused member declaration
#pragma warning restore S4487 // Unread "private" fields should be removed
#pragma warning restore IDE0052 // Remove unread private members
    private readonly IMediator _mediator;
    private readonly DirectSumRequest _directSumRequest = new()
    {
        A = 1,
        B = 2
    };
    private readonly DirectSumRequestWithReply _directSumRequestWithReply = new()
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

    public Program()
    {
        Console.WriteLine(_directSumTransceiver);
        Console.WriteLine(_directSumTransceiverWithoutMetrics);
        Console.WriteLine(_directSumTransceiverWithReply);
        Console.WriteLine(_sslTransceiver);
        Console.WriteLine(_tcpTransceiver);
        _mediator = BuildMediatR();
    }

    //------------------------------Transceiver benchmarks------------------------------
    private static ITransceiver<DirectSumRequest, DirectSumResponse> BuildDirectTransceiver()
    {
        ServiceCollection services = [];
        _ = services.AddLogging(builder => builder.AddConsole());
        _ = services.AddTransceiver(t => t.ConfigureDirectProtocol(), Assembly.GetExecutingAssembly());
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        serviceProvider.ConfigureTransceiverProvider(Assembly.GetExecutingAssembly());
        return serviceProvider.GetRequiredService<ITransceiver<DirectSumRequest, DirectSumResponse>>();
    }

    private static ITransceiver<DirectSumRequest, DirectSumResponse> BuildDirectTransceiverWithoutMetrics()
    {
        ServiceCollection services = [];
        _ = services.AddLogging(builder => builder.AddConsole());
        _ = services.AddTransceiver(t => t.ConfigureDirectProtocol(), Assembly.GetExecutingAssembly());
        _ = services.RemoveAll<IPipelineProcessor<DirectSumRequest, DirectSumResponse>>();
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        serviceProvider.ConfigureTransceiverProvider(Assembly.GetExecutingAssembly());
        return serviceProvider.GetRequiredService<ITransceiver<DirectSumRequest, DirectSumResponse>>();
    }

    private static ITransceiver<DirectSumRequestWithReply, DirectSumResponseWithReply> BuildDirectTransceiverWithReply()
    {
        ServiceCollection services = [];
        _ = services.AddLogging(builder => builder.AddConsole());
        _ = services.AddTransceiver(t => t.ConfigureDirectProtocol(), Assembly.GetExecutingAssembly());
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        serviceProvider.ConfigureTransceiverProvider(Assembly.GetExecutingAssembly());
        return serviceProvider.GetRequiredService<ITransceiver<DirectSumRequestWithReply, DirectSumResponseWithReply>>();
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
            cfg.CertificateThumbprint = "34abfd4fc9e26a3315e1c398f51ebfc41ba0d553";
        });
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        serviceProvider.ConfigureTransceiverProvider(Assembly.GetExecutingAssembly());
        return serviceProvider.GetRequiredService<ITransceiver<SslSumRequest, SslSumResponse>>();
    }

    private static ITransceiver<TcpSumRequest, TcpSumResponse> BuildTcpTransceiver()
    {
        ServiceCollection services = [];
        _ = services.AddLogging(builder => builder.AddConsole());
        _ = services.AddTransceiver(t => {
            ITransceiverSetup setup = t.ConfigureTcp(new(IPAddress.Loopback, 1125));
            setup.SetupServer(false);
            setup.SetupClient();
        }, Assembly.GetExecutingAssembly());
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        serviceProvider.ConfigureTransceiverProvider(Assembly.GetExecutingAssembly());
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

    private static async Task<double> RunTransceiverTest<TRequest, TResponse>(ITransceiver<TRequest, TResponse> transceiver, TRequest request)
    {
        using CancellationTokenSource source1 = new();
        source1.CancelAfter(TimeSpan.FromMinutes(2));
        //warmup
        while (!source1.IsCancellationRequested)
        {
            _ = await transceiver.TransceiveOnceAsync(request, CancellationToken.None);
        }

        int n = 0;
        using CancellationTokenSource source2 = new();
        source2.CancelAfter(TimeSpan.FromMinutes(2));
        long startTime = Stopwatch.GetTimestamp();
        while (!source2.IsCancellationRequested)
        {
            _ = await transceiver.TransceiveOnceAsync(request, CancellationToken.None);
            n++;
        }
        TimeSpan elapsed = Stopwatch.GetElapsedTime(startTime);
        return n / elapsed.TotalSeconds;
    }

    private static async Task<double> RunMediatRTest(IMediator mediator)
    {
        Program program = new();
        using CancellationTokenSource source1 = new();
        source1.CancelAfter(TimeSpan.FromMinutes(2));
        //warmup
        while (!source1.IsCancellationRequested)
        {
            _ = await mediator.Send(program._directSumRequest, CancellationToken.None);
        }

        int n = 0;
        using CancellationTokenSource source2 = new();
        source2.CancelAfter(TimeSpan.FromMinutes(2));
        long startTime = Stopwatch.GetTimestamp();
        while (!source2.IsCancellationRequested)
        {
            _ = await mediator.Send(program._directSumRequest, CancellationToken.None);
            n++;
        }
        TimeSpan elapsed = Stopwatch.GetElapsedTime(startTime);
        return n / elapsed.TotalSeconds;
    }

    private static async Task Main()
    {
        Program program = new();


        double sslThroughput = await RunTransceiverTest(BuildSslTransceiver(), program._sslSumRequest);

        double tcpThroughput = await RunTransceiverTest(BuildTcpTransceiver(), program._tcpSumRequest);

        double mediatRThroughput = await RunMediatRTest(program._mediator);

        double directThroughputNoMetrics = await RunTransceiverTest(BuildDirectTransceiverWithoutMetrics(), program._directSumRequest);

        double directThroughput = await RunTransceiverTest(BuildDirectTransceiver(), program._directSumRequest);

        double directWithReplyThroughput = await RunTransceiverTest(BuildDirectTransceiverWithReply(), program._directSumRequestWithReply);


        Console.WriteLine($"Direct throughput without metrics: {directThroughputNoMetrics} requests/sec. "); //4345653.299779383
        Console.WriteLine($"Direct throughput: {directThroughput} requests/sec. "); //2730201.623160326
        Console.WriteLine($"Direct throughput with reply: {directWithReplyThroughput} requests/sec. "); //2651022.122785491
        Console.WriteLine($"TCP throughput: {tcpThroughput} requests/sec. ");
        Console.WriteLine($"SSL throughput: {sslThroughput} requests/sec. ");
        Console.WriteLine($"MediatR throughput: {mediatRThroughput} requests/sec. ");
    }
}
