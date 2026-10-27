// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using BenchmarkDotNet.Attributes;
using Grpc.Net.Client;
using SumAPI;

namespace Transceiver.Benchmarks;

[MemoryDiagnoser]
[MarkdownExporterAttribute.GitHub()]
[RankColumn, MinColumn, MaxColumn, Q1Column, Q3Column, AllStatisticsColumn]
public class TransceiverBenchmarksGrpc
{
#pragma warning disable S1075 // URIs should not be hardcoded
    private const string UriString = "https://localhost:7124";
#pragma warning restore S1075 // URIs should not be hardcoded

    private GrpcChannel _channel = default!;
    private Summer.SummerClient _client = default!;
    private readonly SumRequest _request = new() { A = 1, B = 2 };

    [GlobalSetup]
    public void Setup()
    {
        _channel = GrpcChannel.ForAddress(UriString);
        _client = new Summer.SummerClient(_channel);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _channel.Dispose();
    }

    [Benchmark]
    public Task<SumReply> GrpcSum()
    {
        return _client.SumAsync(_request).ResponseAsync;
    }
}
