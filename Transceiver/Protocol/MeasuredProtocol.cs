// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Transceiver.Protocol;

internal class MeasuredProtocol : ITransceiverProtocol, IDisposable
{
    private readonly ITransceiverProtocol _protocol;
    private readonly Meter _meter;
    private readonly Histogram<double> _sendHistogram;
    private readonly Histogram<double> _receiveHistogram;
    private bool disposedValue;

    public MeasuredProtocol(ITransceiverProtocol protocol)
    {
        _protocol = protocol;
        _meter = new Meter("Transceiver", "0.1.0");
        _sendHistogram = _meter.CreateHistogram<double>("transceiver_send_object_duration", "ms", "Duration of sending object to client in milliseconds");
        _receiveHistogram = _meter.CreateHistogram<double>("transceiver_receive_object_duration", "ms", "Duration of receiving object from client in milliseconds");
    }

    public IAsyncEnumerable<T> ReceiveObjectsAsync<T>(Guid requestId, CancellationToken cancellationToken) where T : IIdentifiable
    {
        return _protocol.ReceiveObjectsAsync<T>(requestId, cancellationToken);
    }

    public async Task SendObjectToClientAsync<T>(T data, CancellationToken cancellationToken) where T : IIdentifiable
    {
        long now = Stopwatch.GetTimestamp();
        await _protocol.SendObjectToClientAsync(data, cancellationToken);
        long elapsed = Stopwatch.GetTimestamp() - now;
        _sendHistogram.Record(TimeSpan.FromTicks(elapsed).TotalMilliseconds);
    }

    public async Task SendObjectToServerAsync<T>(T data, CancellationToken cancellationToken) where T : IIdentifiable
    {
        long now = Stopwatch.GetTimestamp();
        await _protocol.SendObjectToServerAsync(data, cancellationToken);
        long elapsed = Stopwatch.GetTimestamp() - now;
        _receiveHistogram.Record(TimeSpan.FromTicks(elapsed).TotalMilliseconds);
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

    ~MeasuredProtocol()
    {
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
