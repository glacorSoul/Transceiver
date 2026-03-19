// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using System.Net.WebSockets;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Transceiver.Websockets;

internal class WebsocketsSetup : BaseTransceiverSetup
{
    private readonly Uri _uri;
    public WebsocketsSetup(Type transceiverType, IServiceCollection services, Uri uri)
        : base(transceiverType, services)
    {
        _uri = uri;
    }

    private WebsocketsProtocol CreateSocketProtocol(IServiceProvider provider, bool isServer)
    {
        IMessageProcessor messageProcessor = provider.GetRequiredService<IMessageProcessor>();
        ICertificateLoader certificateLoader = provider.GetRequiredService<ICertificateLoader>();
        ISerializer serializer = provider.GetRequiredService<ISerializer>();
        ILogger<WebsocketsProtocol> logger = provider.GetRequiredService<ILogger<WebsocketsProtocol>>();
        IOptions<TransceiverConfiguration> configuration = provider.GetRequiredService<IOptions<TransceiverConfiguration>>();

        KestrelWebsocketSource? websocketSource = isServer ? new(_uri, certificateLoader, configuration) : null;
        WebsocketsProtocol protocol = new(_uri, messageProcessor, serializer, logger, websocketSource, configuration);
        return protocol;
    }

    public override void SetupClient()
    {
        base.SetupClient();
        Services.TryAddSingleton<ITransceiverProtocol>((provider) =>
        {
            WebsocketsProtocol protocol = CreateSocketProtocol(provider, false);
            WebSocketStream stream = protocol.SetupWriterAsync(CancellationToken.None).GetAwaiter().GetResult();
            _ = protocol.ReceiveMessagesAsync(stream, CancellationToken.None);
            return protocol;
        });
    }

    public override void SetupServer(bool serverOnly)
    {
        base.SetupServer(serverOnly);
        _ = Services.AddSingleton<ITransceiverProtocol>((provider) =>
        {
            WebsocketsProtocol protocol = CreateSocketProtocol(provider, true);
            _ = protocol.ReceiveMessagesAsync(CancellationToken.None);
            if (!serverOnly)
            {
                WebSocketStream stream = protocol.SetupWriterAsync(CancellationToken.None).GetAwaiter().GetResult();
                _ = protocol.ReceiveMessagesAsync(stream, CancellationToken.None);
            }
            return protocol;
        });
    }
}
