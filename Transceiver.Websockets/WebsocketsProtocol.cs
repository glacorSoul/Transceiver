// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using System.Net.WebSockets;
using Microsoft.Extensions.Options;

namespace Transceiver;

public class WebsocketsProtocol : ReceiveMessagesProtocol<WebSocketStream>, IDisposable
{
    private readonly Uri _serverEndpoint;
    private readonly SemaphoreSlim _writeLock = new(1, 1);
    private readonly KestrelWebsocketSource? _kestrelWebsocketSource;
    private readonly Lazy<Task<WebSocketStream>> _clientStream;
    private bool disposedValue;

    public WebsocketsProtocol(Uri serverEndpoint,
        IMessageProcessor messageProcessor,
        ISerializer serializer,
        ILogger logger,
        KestrelWebsocketSource? kestrelWebsocketSource,
        IOptions<TransceiverConfiguration> configuration)
        : base(messageProcessor, serializer, logger, configuration)
    {
        _serverEndpoint = serverEndpoint;
        _kestrelWebsocketSource = kestrelWebsocketSource;
        _clientStream = new Lazy<Task<WebSocketStream>>(async () =>
        {
            ClientWebSocket client = new();
            await client.ConnectAsync(_serverEndpoint, CancellationToken.None);
            WebSocketStream stream = WebSocketStream.Create(client, WebSocketMessageType.Binary, Configuration.Value.RequestTimeout);
            return stream;
        });
    }

    public override Task<WebSocketStream> SetupWriterAsync(CancellationToken cancellationToken)
    {
        return _clientStream.Value;
    }

    protected override async Task<(int, object)> ReadAsync(WebSocketStream reader, byte[] buffer, CancellationToken cancellationToken)
    {
        int bytesRead = await reader.ReadAsync(buffer, cancellationToken);
        return (bytesRead, reader);
    }

    protected override async Task<WebSocketStream> SetupReadAsync(CancellationToken cancellationToken)
    {
        if (_kestrelWebsocketSource is null)
        {
            throw new InvalidOperationException("Could not start websocket server because Transceiver is configured as client only.");
        }
        WebSocketStream stream = await _kestrelWebsocketSource.AcceptWebSocketAsync(cancellationToken);
        return stream;
    }

    protected override async Task WriteAsync(WebSocketStream transceiver, object client, byte[] data, CancellationToken cancellationToken)
    {
        await _writeLock.WaitAsync(cancellationToken);
        try
        {
            await transceiver.WriteAsync(data, cancellationToken);
            await transceiver.FlushAsync(cancellationToken);
        }
        finally
        {
            _ = _writeLock.Release();
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _writeLock.Dispose();
            }

            disposedValue = true;
        }
    }

    ~WebsocketsProtocol()
    {
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}