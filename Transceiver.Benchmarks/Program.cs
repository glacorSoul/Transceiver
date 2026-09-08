// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Transceiver.Benchmarks;

internal class Program
{
#pragma warning disable S1075 // URIs should not be hardcoded
    private const string UriString = "https://localhost:7124";
#pragma warning restore S1075 // URIs should not be hardcoded
    private readonly ITransceiver<DirectSumRequest, DirectSumResponse> _directSumTransceiver = default!;
    private readonly ITransceiver<DirectSumRequest, DirectSumResponse> _directSumTransceiverWithoutMetrics = default!;
    private readonly ITransceiver<DirectSumRequestWithReply, DirectSumResponseWithReply> _directSumTransceiverWithReply = default!;
    private readonly ITransceiver<SslSumRequest, SslSumResponse> _sslTransceiver = default!;
    private readonly ITransceiver<TcpSumRequest, TcpSumResponse> _tcpTransceiver = default!;
    private static readonly TimeSpan WarmupTime = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan ExecutionTime = TimeSpan.FromSeconds(10);
    private readonly TcpSumRequest _tcpSumRequest = new()
    {
        A = 1,
        B = 2
    };
    private readonly SslSumRequest _sslSumRequest = new()
    {
        A = 1,
        B = 2
    };

    public Program()
    {
        Console.Write(_directSumTransceiver);
        Console.Write(_directSumTransceiverWithoutMetrics);
        Console.Write(_directSumTransceiverWithReply);
        Console.Write(_sslTransceiver);
        Console.Write(_tcpTransceiver);
    }

    //------------------------------Transceiver benchmarks------------------------------
    private static ITransceiver<TcpSumRequest, TcpSumResponse> BuildTcpTransceiver(CancellationToken cancellationToken)
    {
        ServiceCollection services = [];
        _ = services.AddLogging(builder => builder.AddConsole());
        _ = services.AddTransceiver(t =>
        {
            ITransceiverSetup setup = t.ConfigureTcp(new(IPAddress.Loopback, 11125));
            setup.SetupServer(false, cancellationToken);
            setup.SetupClient(cancellationToken);
        }, Assembly.GetExecutingAssembly());
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        serviceProvider.ConfigureTransceiverProvider();
        return serviceProvider.GetRequiredService<ITransceiver<TcpSumRequest, TcpSumResponse>>();
    }

    private static ITransceiver<SslSumRequest, SslSumResponse> BuildSSLTransceiver(CancellationToken cancellationToken)
    {
        ServiceCollection services = [];
        _ = services.AddLogging(builder => builder.AddConsole());
        _ = services.AddTransceiver(t =>
        {
            ITransceiverSetup setup = t.ConfigureTcp(new(IPAddress.Loopback, 12125));
            setup.SetupServer(false, cancellationToken);
            setup.SetupClient(cancellationToken);
        }, Assembly.GetExecutingAssembly());
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        serviceProvider.ConfigureTransceiverProvider();
        return serviceProvider.GetRequiredService<ITransceiver<SslSumRequest, SslSumResponse>>();
    }

    private static ITransceiver<DirectSumRequest, DirectSumResponse> BuildDirectTransceiver()
    {
        ServiceCollection services = [];
        _ = services.AddLogging(builder => builder.AddConsole());
        _ = services.AddTransceiver(transceiverConfiguration => transceiverConfiguration.ConfigureDirectProtocol(), Assembly.GetExecutingAssembly());
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        return serviceProvider.GetRequiredService<ITransceiver<DirectSumRequest, DirectSumResponse>>();
    }

    private static ITransceiver<DirectSumRequest, DirectSumResponse> BuildDirectTransceiverWithoutMetrics()
    {
        ServiceCollection services = [];
        _ = services.AddLogging(builder => builder.AddConsole());
        _ = services.AddTransceiver(transceiverConfiguration => transceiverConfiguration.ConfigureDirectProtocol(), Assembly.GetExecutingAssembly());
        _ = services.RemoveAll<IPipelineProcessor<DirectSumRequest, DirectSumResponse>>();
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        return serviceProvider.GetRequiredService<ITransceiver<DirectSumRequest, DirectSumResponse>>();
    }

    private static IMediator BuildMediatR()
    {
        ServiceCollection services = [];
        _ = services.AddLogging(builder => builder.AddConsole());
        _ = services.AddMediatR(Assembly.GetExecutingAssembly());
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        return serviceProvider.GetRequiredService<IMediator>();
    }

    private static async Task<double> RunHTTPTest(string uriString)
    {
        HttpClient httpClient = new()
        {
            BaseAddress = new Uri(uriString)
        };
        using CancellationTokenSource source1 = new();
        source1.CancelAfter(WarmupTime);
        //warmup
        while (!source1.IsCancellationRequested)
        {
            _ = await httpClient.GetFromJsonAsync<int>("/sum/1/2");
        }

        int n = 0;
        using CancellationTokenSource source2 = new();
        source2.CancelAfter(ExecutionTime);
        long startTime = Stopwatch.GetTimestamp();
        while (!source2.IsCancellationRequested)
        {
            _ = await httpClient.GetFromJsonAsync<int>("/sum/1/2");
            n++;
        }
        TimeSpan elapsed = Stopwatch.GetElapsedTime(startTime);
        return n / elapsed.TotalSeconds;
    }

    private static async Task<double> RunMediatRTest()
    {
        IMediator mediator = BuildMediatR();
        using CancellationTokenSource source1 = new();
        source1.CancelAfter(WarmupTime);
        DirectSumRequest request = new() { A = 1, B = 2 };
        //warmup
        while (!source1.IsCancellationRequested)
        {
            _ = await mediator.Send(request, CancellationToken.None);
        }

        int n = 0;
        using CancellationTokenSource source2 = new();
        source2.CancelAfter(ExecutionTime);
        long startTime = Stopwatch.GetTimestamp();
        while (!source2.IsCancellationRequested)
        {
            _ = await mediator.Send(request, CancellationToken.None);
            n++;
        }
        TimeSpan elapsed = Stopwatch.GetElapsedTime(startTime);
        return n / elapsed.TotalSeconds;
    }

    private static async Task<double> RunTransceiverTest<TRequest, TResponse>(ITransceiver<TRequest, TResponse> transceiver, TRequest request)
    {
        using CancellationTokenSource source1 = new();
        source1.CancelAfter(WarmupTime);
        //warmup
        while (!source1.IsCancellationRequested)
        {
            _ = await transceiver.TransceiveOnceAsync(request, CancellationToken.None);
        }

        int n = 0;
        using CancellationTokenSource source2 = new();
        source2.CancelAfter(ExecutionTime);
        long startTime = Stopwatch.GetTimestamp();
        while (!source2.IsCancellationRequested)
        {
            _ = await transceiver.TransceiveOnceAsync(request, CancellationToken.None);
            n++;
        }
        TimeSpan elapsed = Stopwatch.GetElapsedTime(startTime);
        return n / elapsed.TotalSeconds;
    }

    private static async Task Main()
    {
        Program program = new();

        Console.WriteLine("Running TCP transceiver benchmark...");
        double tcpThroughput = await RunTransceiverTest(BuildTcpTransceiver(CancellationToken.None), program._tcpSumRequest);

        Console.WriteLine("Running MediatR benchmark...");
        double mediatRThroughput = await RunMediatRTest();

        Console.WriteLine("Running SSL transceiver benchmark...");
        double sslThroughput = await RunTransceiverTest(BuildSSLTransceiver(CancellationToken.None), program._sslSumRequest);

        Console.WriteLine("Running Direct transceiver benchmark without metrics...");
        double directWithoutMetricsThroughput = await RunTransceiverTest(BuildDirectTransceiverWithoutMetrics(), new DirectSumRequest { A = 1, B = 2 });

        Console.WriteLine("Running Direct transceiver benchmark...");
        double directThroughput = await RunTransceiverTest(BuildDirectTransceiver(), new DirectSumRequest { A = 1, B = 2 });

        Console.WriteLine("Running HTTP benchmark...");
        double httpThroughput = await RunHTTPTest(UriString);

        Console.WriteLine($"Direct without metrics throughput: {directWithoutMetricsThroughput} requests/s");
        Console.WriteLine($"MediatR throughput: {mediatRThroughput} requests/s");
        Console.WriteLine($"Direct throughput: {directThroughput} requests/s");
        Console.WriteLine($"TCP throughput: {tcpThroughput} requests/s");
        Console.WriteLine($"SSL throughput: {sslThroughput} requests/s");
        Console.WriteLine($"HTTP throughput: {httpThroughput} requests/s");
    }
}
