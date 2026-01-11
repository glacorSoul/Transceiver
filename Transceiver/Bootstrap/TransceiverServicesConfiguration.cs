// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace Transceiver;

public class TransceiverServicesConfiguration
{
    private IServiceCollection _services;

    internal TransceiverServicesConfiguration(IServiceCollection services, Type type)
    {
        _services = services;
        Type = type;
    }

    public Type Type { get; }

    public ITransceiverSetup ConfigureCustomProtocol(Type protocolType)
    {
        CustomProtocolSetup setup = new(Type, protocolType, _services);
        return setup;
    }

    public ITransceiverSetup ConfigureDelegateToMessageProcessor()
    {
        _services = _services.AddSingleton<ITransceiverProtocol, DelegateToMessageProcessorProtocol>();
        BaseTransceiverSetup setup = new(Type, _services);
        return setup;
    }

    public ITransceiverSetup ConfigureDirectProtocol()
    {
        DirectProtocolSetup setup = new(Type, _services);
        return setup;
    }

    public ITransceiverSetup ConfigureSsl(IPEndPoint serverEndPoint)
    {
        return ConfigureSsl(serverEndPoint, "localhost");
    }

    public ITransceiverSetup ConfigureSsl(IPEndPoint serverEndPoint, string name)
    {
        SslProtocolSetup setup = new(Type, _services, serverEndPoint, name);
        return setup;
    }

    public ITransceiverSetup ConfigureTcp(IPEndPoint serverEndPoint)
    {
        return ConfigureTcp(serverEndPoint, "localhost");
    }

    public ITransceiverSetup ConfigureTcp(IPEndPoint serverEndPoint, string name)
    {
        ProtocolSpecificSetup setup = new(Type, _services, serverEndPoint, name, ProtocolTypeEnum.Tcp);
        return setup;
    }

    public ITransceiverSetup ConfigureUdp(IPEndPoint serverEndPoint)
    {
        return ConfigureUdp(serverEndPoint, "localhost");
    }

    public ITransceiverSetup ConfigureUdp(IPEndPoint serverEndPoint, string name)
    {
        ProtocolSpecificSetup setup = new(Type, _services, serverEndPoint, name, ProtocolTypeEnum.Udp);
        return setup;
    }

    public ITransceiverSetup ConfigureUnknownSocket()
    {
        SocketsSetup setup = new(Type, _services);
        return setup;
    }
}