// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Transceiver.Serializer;

public class SerializerMeasured : ISerializer, IDisposable
{
    private readonly ISerializer _innerSerializer;
    private readonly Meter _meter;
    private readonly Histogram<double> _deserializeTimeHistogram;
    private readonly Histogram<double> _serializeTimeHistogram;
    private bool disposedValue;

    public SerializerMeasured(ISerializer innerSerializer)
    {
        _innerSerializer = innerSerializer;
        _meter = new("Transceiver", "0.1.0");
        _deserializeTimeHistogram = _meter.CreateHistogram<double>("transceiver_serializer_deserialize_time_ms", "ms", "Time taken to deserialize data");
        _serializeTimeHistogram = _meter.CreateHistogram<double>("transceiver_serializer_serialize_time_ms", "ms", "Time taken to serialize data");
    }

    public object Deserialize(Type type, byte[] data)
    {
        long now = Stopwatch.GetTimestamp();
        object result = _innerSerializer.Deserialize(type, data);
        long elapsed = Stopwatch.GetTimestamp() - now;
        _deserializeTimeHistogram.Record(TimeSpan.FromTicks(elapsed).TotalMilliseconds);
        return result;
    }

    public T Deserialize<T>(byte[] data)
    {
        long now = Stopwatch.GetTimestamp();
        T result = _innerSerializer.Deserialize<T>(data);
        long elapsed = Stopwatch.GetTimestamp() - now;
        _deserializeTimeHistogram.Record(TimeSpan.FromTicks(elapsed).TotalMilliseconds);
        return result;
    }

    public Task<object> DeserializeAsync(Type type, byte[] data, CancellationToken cancellationToken)
    {
        long now = Stopwatch.GetTimestamp();
        Task<object> result = _innerSerializer.DeserializeAsync(type, data, cancellationToken);
        long elapsed = Stopwatch.GetTimestamp() - now;
        _deserializeTimeHistogram.Record(TimeSpan.FromTicks(elapsed).TotalMilliseconds);
        return result;
    }

    public Task<T> DeserializeAsync<T>(byte[] data, CancellationToken cancellationToken)
    {
        long now = Stopwatch.GetTimestamp();
        Task<T> result = _innerSerializer.DeserializeAsync<T>(data, cancellationToken);
        long elapsed = Stopwatch.GetTimestamp() - now;
        _deserializeTimeHistogram.Record(TimeSpan.FromTicks(elapsed).TotalMilliseconds);
        return result;
    }

    public byte[] Serialize<T>(T data)
    {
        long now = Stopwatch.GetTimestamp();
        byte[] result = _innerSerializer.Serialize(data);
        long elapsed = Stopwatch.GetTimestamp() - now;
        _serializeTimeHistogram.Record(TimeSpan.FromTicks(elapsed).TotalMilliseconds);
        return result;
    }

    public Task<byte[]> SerializeAsync(Type type, object data, CancellationToken cancellationToken)
    {
        long now = Stopwatch.GetTimestamp();
        Task<byte[]> result = _innerSerializer.SerializeAsync(type, data, cancellationToken);
        long elapsed = Stopwatch.GetTimestamp() - now;
        _serializeTimeHistogram.Record(TimeSpan.FromTicks(elapsed).TotalMilliseconds);
        return result;
    }

    public Task<byte[]> SerializeAsync<T>(T data, CancellationToken cancellationToken)
    {
        long now = Stopwatch.GetTimestamp();
        Task<byte[]> result = _innerSerializer.SerializeAsync(data, cancellationToken);
        long elapsed = Stopwatch.GetTimestamp() - now;
        _serializeTimeHistogram.Record(TimeSpan.FromTicks(elapsed).TotalMilliseconds);
        return result;
    }

    public byte[] Seriazlize(Type type, object data)
    {
        long now = Stopwatch.GetTimestamp();
        byte[] result = _innerSerializer.Seriazlize(type, data);
        long elapsed = Stopwatch.GetTimestamp() - now;
        _serializeTimeHistogram.Record(TimeSpan.FromTicks(elapsed).TotalMilliseconds);
        return result;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _meter.Dispose();
            }
            disposedValue = true;
        }
    }

    ~SerializerMeasured()
    {
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
