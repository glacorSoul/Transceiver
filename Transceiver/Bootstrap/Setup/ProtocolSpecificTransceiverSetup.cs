// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace Transceiver;

internal class ProtocolSpecificTransceiverSetup : SocketsSetup
{
    private readonly ProtocolTypeEnum _protocolType;
    private readonly IPEndPoint _serverEndPoint;

    public ProtocolSpecificTransceiverSetup(Type transceiverType,
        IServiceCollection services,
        IPEndPoint serverEndPoint,
        ProtocolTypeEnum protocolType) : base(transceiverType, services)
    {
        _serverEndPoint = serverEndPoint;
        _protocolType = protocolType;
    }

    public override void SetupClient()
    {
        base.SetupClient();
        SetupServerSocketProtocol();
    }

    public override void SetupServer(bool serverOnly)
    {
        base.SetupServer(serverOnly);
        SetupServerSocketProtocol();
    }

    private void SetupServerSocketProtocol()
    {
        _ = Services.AddSingleton<ISocketFactory>((provider) =>
        {
            return SocketFactory.GetFactory(_protocolType, _serverEndPoint);
        });
    }
}