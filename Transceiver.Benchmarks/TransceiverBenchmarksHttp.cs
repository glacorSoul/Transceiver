// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using BenchmarkDotNet.Attributes;

namespace Transceiver.Benchmarks;

[MemoryDiagnoser]
[MarkdownExporterAttribute.GitHub()]
[RankColumn, MinColumn, MaxColumn, Q1Column, Q3Column, AllStatisticsColumn]
public class TransceiverBenchmarksHttp : IDisposable
{
#pragma warning disable S1075 // URIs should not be hardcoded
    private const string UriString = "https://localhost:7124";
#pragma warning restore S1075 // URIs should not be hardcoded

    private HttpClient _httpClient = default!;
    private bool disposedValue;

    [GlobalSetup]
    public void Setup()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(UriString)
        };
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _httpClient.Dispose();
    }

    [Benchmark]
    public Task<string> HttpSum()
    {
        return _httpClient.GetStringAsync("/sum/1/2");
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _httpClient.Dispose();
            }
            disposedValue = true;
        }
    }

    ~TransceiverBenchmarksHttp()
    {
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
