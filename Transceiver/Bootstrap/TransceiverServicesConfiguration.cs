// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace Transceiver;

public class TransceiverServicesConfiguration
{
    internal TransceiverServicesConfiguration(IServiceCollection services, Type type)
    {
        Services = services;
        Type = type;
    }

    public Type Type { get; }
    public IServiceCollection Services { get; private set; }

    public ITransceiverSetup ConfigureCustomProtocol(Type protocolType)
    {
        CustomProtocolSetup setup = new(Type, protocolType, Services);
        return setup;
    }

    public ITransceiverSetup ConfigureDelegateToMessageProcessor()
    {
        Services = Services.AddSingleton<ITransceiverProtocol, DelegateToMessageProcessorProtocol>();
        BaseTransceiverSetup setup = new(Type, Services);
        return setup;
    }

    public ITransceiverSetup ConfigureDirectProtocol()
    {
        DirectProtocolSetup setup = new(Type, Services);
        return setup;
    }

    public ITransceiverSetup ConfigureSsl(IPEndPoint serverEndPoint)
    {
        return ConfigureSsl(serverEndPoint, "localhost");
    }

    public ITransceiverSetup ConfigureSsl(IPEndPoint serverEndPoint, string name)
    {
        SslProtocolSetup setup = new(Type, Services, serverEndPoint, name);
        return setup;
    }

    public ITransceiverSetup ConfigureTcp(IPEndPoint serverEndPoint)
    {
        return ConfigureTcp(serverEndPoint, "localhost");
    }

    public ITransceiverSetup ConfigureTcp(IPEndPoint serverEndPoint, string name)
    {
        ProtocolSpecificSetup setup = new(Type, Services, serverEndPoint, name, ProtocolTypeEnum.Tcp);
        return setup;
    }

    public ITransceiverSetup ConfigureUdp(IPEndPoint serverEndPoint)
    {
        return ConfigureUdp(serverEndPoint, "localhost");
    }

    public ITransceiverSetup ConfigureUdp(IPEndPoint serverEndPoint, string name)
    {
        ProtocolSpecificSetup setup = new(Type, Services, serverEndPoint, name, ProtocolTypeEnum.Udp);
        return setup;
    }

    public ITransceiverSetup ConfigureUnknownSocket()
    {
        SocketsSetup setup = new(Type, Services);
        return setup;
    }
}