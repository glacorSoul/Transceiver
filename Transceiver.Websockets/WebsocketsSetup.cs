// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

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

    public override void SetupClient(CancellationToken cancellationToken)
    {
        base.SetupClient(cancellationToken);
        Services.TryAddSingleton<ITransceiverProtocol>((provider) =>
        {
            WebsocketsProtocol protocol = CreateSocketProtocol(provider, false);
            _ = protocol.SetupWriterAsync(CancellationToken.None).ContinueWith(stream =>
            {
                _ = protocol.ServerReceiveMessagesAsync(stream.Result, CancellationToken.None);
            });
            return protocol;
        });
    }

    public override void SetupServer(bool serverOnly, CancellationToken cancellationToken)
    {
        base.SetupServer(serverOnly, cancellationToken);
        _ = Services.AddSingleton<ITransceiverProtocol>((provider) =>
        {
            WebsocketsProtocol protocol = CreateSocketProtocol(provider, true);
            _ = protocol.ReceiveMessagesAsync(CancellationToken.None);
            if (!serverOnly)
            {
                _ = protocol.SetupWriterAsync(CancellationToken.None).ContinueWith(stream =>
                {
                    _ = protocol.ServerReceiveMessagesAsync(stream.Result, CancellationToken.None);
                });
            }
            return protocol;
        });
    }
}
