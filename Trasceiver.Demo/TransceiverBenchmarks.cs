// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Exporters;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Reflection;

namespace Transceiver.Demo;

[MemoryDiagnoser]
[RPlotExporter]
[HtmlExporter]
[MarkdownExporterAttribute.Default]
[MarkdownExporterAttribute.GitHub]
[XmlExporter]
[CsvExporter]
public class TransceiverBenchmarks : IProcessor<SumRequest, SumResponse>, IDisposable
{
    private CancellationTokenSource _cts = default!;
    private bool _disposed;
    private Task _server = default!;
    private ITransceiver<SumRequest, SumResponse> _transceiver = default!;

    ~TransceiverBenchmarks()
    {
        Dispose(false);
    }

    [Params("Channels"/*, "DomainSockets", "Udp", "Tcp"*/)]
    public string Implementation { get; set; } = default!;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        try
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _server?.Wait();
        }
        catch (Exception)
        {
            _ = 1;
        }
    }

    public Task<SumResponse> ProcessRequest(SumRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new SumResponse(request.A + request.B));
    }

    [Benchmark]
    public async Task SendAsync()
    {
        _ = await _transceiver.SendToServerAsync(new SumRequest(1, 2), _cts.Token);
    }

    [GlobalSetup]
    public void Setup()
    {
        Assembly assembly = typeof(TransceiverBenchmarks).Assembly;
        ServiceCollection udpServices = new();
        _ = udpServices.AddTransceiver(config => config.ConfigureUdp(new(IPAddress.Loopback, 9998)), assembly);

        ServiceCollection tcpServices = new();
        _ = tcpServices.AddTransceiver(config => config.ConfigureTcp(new(IPAddress.Loopback, 9999)), assembly);

        _cts = new CancellationTokenSource();
        _transceiver = Implementation switch
        {
            "Udp" => udpServices.BuildServiceProvider().GetService<ITransceiver<SumRequest, SumResponse>>()!,
            "Tcp" => tcpServices.BuildServiceProvider().GetService<ITransceiver<SumRequest, SumResponse>>()!,
            _ => throw new NotSupportedException()
        };

        _server = Task.Run(async () =>
        {
            try
            {
                await _transceiver.StartProcessingRequestsAsync(this, _cts.Token);
            }
            catch (Exception)
            {
                _ = 1;
            }
        }, _cts.Token);
    }

    [Benchmark]
    public async Task<int> Transcieve()
    {
        SumResponse result = await _transceiver.TransceiveOnceAsync(new SumRequest(1, 2), _cts.Token);
        return result.Result;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }
        if (disposing)
        {
            _cts?.Dispose();
            _server?.Dispose();
        }
        _disposed = true;
    }
}