// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using NetMQ;
using NetMQ.Sockets;
using System.Net;

namespace Transceiver.ZeroMq;

public sealed class ZeroMqProtocol : ITransceiverProtocol, IDisposable
{
    private readonly CancellationTokenSource _cts;
    private readonly Task _initializeTask;
    private readonly IMessageProcessor _messageProcessor;
    private readonly PublisherSocket _pubSocket;
    private readonly ISerializer _serializer;
    private readonly SubscriberSocket _subSocket;
    private bool _disposed;

    public ZeroMqProtocol(IPEndPoint endpoint, ISerializer serializer, IMessageProcessor messageProcessor)
    {
        _messageProcessor = messageProcessor;
        _serializer = serializer;
        _cts = new();
        _pubSocket = new();
        _subSocket = new();
        _initializeTask = Task.Run(() =>
        {
            _pubSocket.Bind($"tcp://127.0.0.1:{endpoint.Port}");
            _subSocket.Connect($"tcp://127.0.0.1:{endpoint.Port}");
            _subSocket.Subscribe("");
            Thread.Sleep(500);
        });
        _ = Task.Run(ProcessMessage);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public AsyncSource<T> ReceiveObjects<T>(Guid requestId) where T : IIdentifiable
    {
        return _messageProcessor.AddRequester<T>(requestId);
    }

    public async Task SendObjectAsync<T>(Guid requestId, T data)
    {
        await _initializeTask;
        byte[] buffer = _serializer.Serialize(data);
        TransceiverHeader header = TransceiverHeader.CreateHeader(data!.GetType(), buffer.Length, requestId);

        lock (_pubSocket)
        {
            _pubSocket.SendMoreFrame(typeof(T).ToShortPrettyString().ToLettersOnly())
                .SendMoreFrame(header.ToArray())
                .SendFrame(buffer);
        }
    }

    public Task SendObjectToClientAsync<T>(T data, CancellationToken cancellationToken) where T : IIdentifiable
    {
        return SendObjectAsync(data.Id, data);
    }

    public Task SendObjectToServerAsync<T>(T data, CancellationToken cancellationToken) where T : IIdentifiable
    {
        return SendObjectAsync(data.Id, data);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _subSocket.Dispose();
            _pubSocket.Dispose();
            _cts.Cancel();
            _cts.Dispose();
        }

        _disposed = true;
    }

    private async Task ProcessMessage()
    {
        await _initializeTask;
        while (!_cts.Token.IsCancellationRequested)
        {
            string topic = _subSocket.ReceiveFrameString();
            if (string.IsNullOrEmpty(topic))
            {
                continue;
            }
            TransceiverHeader header = new(_subSocket.ReceiveFrameBytes());
            byte[] messageReceived = _subSocket.ReceiveFrameBytes();
            TransceiverMessage message = new(header, messageReceived);

            await _messageProcessor.ProcessMessageAsync(message, _cts.Token);
        }
    }
}