// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;

namespace Transceiver;

internal sealed class SslProtocolSetup : ProtocolSpecificTransceiverSetup
{
    public SslProtocolSetup(Type transceiverType, IServiceCollection services, IPEndPoint serverEndpoint)
        : base(transceiverType, services, serverEndpoint, ProtocolTypeEnum.Ssl)
    {
    }

    public sealed override void SetupClient()
    {
        base.SetupClient();
        Services.TryAddSingleton<ITransceiverProtocol>((provider) =>
        {
            IMessageProcessor messageProcessor = provider.GetRequiredService<IMessageProcessor>();
            ISerializer serializer = provider.GetRequiredService<ISerializer>();
            ISocketFactory factory = provider.GetRequiredService<ISocketFactory>();
            ILogger<TransceiverSslProtocol> logger = provider.GetRequiredService<ILogger<TransceiverSslProtocol>>();
            IOptions<TransceiverConfiguration> configuration = provider.GetRequiredService<IOptions<TransceiverConfiguration>>();
            TransceiverSslProtocol protocol = new(factory, messageProcessor, serializer, logger, configuration);
            Stream stream = protocol.SetupWriterAsync(CancellationToken.None).GetAwaiter().GetResult();
            _ = protocol.ReceiveMessagesAsync(stream, CancellationToken.None);
            return protocol;
        });
    }

    public sealed override void SetupServer(bool serverOnly)
    {
        base.SetupServer(serverOnly);
        _ = Services.AddSingleton<ITransceiverProtocol>((provider) =>
        {
            IMessageProcessor messageProcessor = provider.GetRequiredService<IMessageProcessor>();
            ISerializer serializer = provider.GetRequiredService<ISerializer>();
            ISocketFactory factory = provider.GetRequiredService<ISocketFactory>();
            ILogger<TransceiverSslProtocol> logger = provider.GetRequiredService<ILogger<TransceiverSslProtocol>>();
            IOptions<TransceiverConfiguration> configuration = provider.GetRequiredService<IOptions<TransceiverConfiguration>>();
            TransceiverSslProtocol protocol = new(factory, messageProcessor, serializer, logger, configuration);
            _ = protocol.ReceiveMessagesAsync(CancellationToken.None);
            Stream stream = protocol.SetupWriterAsync(CancellationToken.None).GetAwaiter().GetResult();
            _ = protocol.ReceiveMessagesAsync(stream, CancellationToken.None);
            return protocol;
        });
    }
}