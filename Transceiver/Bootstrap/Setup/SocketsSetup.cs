// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Sockets;

namespace Transceiver;

public class SocketsSetup : BaseTransceiverSetup
{
    public SocketsSetup(Type transceiverType, IServiceCollection services)
        : base(transceiverType, services)
    {
    }

    public override void SetupClient()
    {
        base.SetupClient();
        Services.TryAddSingleton<ITransceiverProtocol>((provider) =>
        {
            IMessageProcessor messageProcessor = provider.GetRequiredService<IMessageProcessor>();
            ISerializer serializer = provider.GetRequiredService<ISerializer>();
            ISocketFactory factory = provider.GetRequiredService<ISocketFactory>();
            ILogger<TransceiverSocketProtocol> logger = provider.GetRequiredService<ILogger<TransceiverSocketProtocol>>();
            IOptions<TransceiverConfiguration> configuration = provider.GetRequiredService<IOptions<TransceiverConfiguration>>();
            TransceiverSocketProtocol protocol = new(messageProcessor, factory, serializer, logger, configuration);
            Socket socket = protocol.SetupWriterAsync(CancellationToken.None).GetAwaiter().GetResult();
            _ = protocol.ReceiveMessagesAsync(socket, CancellationToken.None);
            return protocol;
        });
    }

    public override void SetupServer(bool serverOnly)
    {
        base.SetupServer(serverOnly);
        _ = Services.AddSingleton<ITransceiverProtocol>((provider) =>
        {
            IMessageProcessor messageProcessor = provider.GetRequiredService<IMessageProcessor>();
            ISerializer serializer = provider.GetRequiredService<ISerializer>();
            ISocketFactory factory = provider.GetRequiredService<ISocketFactory>();
            ILogger<TransceiverSocketProtocol> logger = provider.GetRequiredService<ILogger<TransceiverSocketProtocol>>();
            IOptions<TransceiverConfiguration> configuration = provider.GetRequiredService<IOptions<TransceiverConfiguration>>();
            TransceiverSocketProtocol protocol = new(messageProcessor, factory, serializer, logger, configuration);
            _ = protocol.ReceiveMessagesAsync(CancellationToken.None);
            if (!serverOnly)
            {
                Socket socket = protocol.SetupWriterAsync(CancellationToken.None).GetAwaiter().GetResult();
                _ = protocol.ReceiveMessagesAsync(socket, CancellationToken.None);
            }
            return protocol;
        });
    }
}