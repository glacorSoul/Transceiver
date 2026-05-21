// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using System.Collections.Concurrent;
using System.Net.WebSockets;
using Microsoft.Extensions.Options;

namespace Transceiver;

public class KestrelWebsocketSource : IDisposable
{
    private readonly Uri _serverEndpoint;
    private readonly ICertificateLoader _certificateLoader;
    private readonly IOptions<TransceiverConfiguration> _configuration;
    private readonly WebApplication _app;
    private readonly BlockingCollection<WebSocketStream> _pendingConnections = [];
    private readonly SemaphoreSlim _semaphore;
    private bool disposedValue;

    public KestrelWebsocketSource(Uri serverEndpoint,
        ICertificateLoader certificateLoader,
        IOptions<TransceiverConfiguration> configuration)
    {
        _serverEndpoint = serverEndpoint;
        _certificateLoader = certificateLoader;
        _configuration = configuration;
        _semaphore = new(0, short.MaxValue);
        _app = BuildApp();
        _ = _app.RunAsync();
    }

    private void MapWebSocketsEndpoint(WebApplication app, string path)
    {
        _ = app.Map(path, async context =>
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }

            WebSocket socket = await context.WebSockets.AcceptWebSocketAsync();
            WebSocketStream stream = WebSocketStream.Create(socket, WebSocketMessageType.Binary, _configuration.Value.RequestTimeout);
            _pendingConnections.Add(stream);
            _ = _semaphore.Release();

            while (socket.State == WebSocketState.Open)
            {
                await Task.Delay(TimeSpan.FromSeconds(10));
            }
        });
    }

    public async Task<WebSocketStream> AcceptWebSocketAsync(CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);
        WebSocketStream stream = _pendingConnections.Take(cancellationToken);
        return stream;
    }

    private WebApplication BuildApp()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();

        int port = _serverEndpoint.Port;
        _ = builder.WebHost.ConfigureKestrel(options =>
        {
            options.ListenAnyIP(port, listen =>
            {
                _ = listen.UseHttps(_certificateLoader.LoadCertificate(_configuration.Value.CertificateThumbprint));
            });
        });

        WebSocketOptions webSocketOptions = new()
        {
            KeepAliveInterval = _configuration.Value.RequestTimeout
        };

        WebApplication app = builder.Build();
        _ = app.UseWebSockets(webSocketOptions);

        string path = _serverEndpoint.AbsolutePath;
        if (string.IsNullOrEmpty(path))
        {
            path = "/";
        }
        string mapPath = path == "/" ? "/" : path.TrimEnd('/');

        MapWebSocketsEndpoint(app, mapPath);

        return app;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _semaphore.Dispose();
            }

            disposedValue = true;
        }
    }

    ~KestrelWebsocketSource()
    {
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
