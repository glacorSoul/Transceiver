// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using Microsoft.Extensions.DependencyInjection;

namespace Transceiver;

public class CustomProtocolSetup : BaseTransceiverSetup
{
    private readonly Type _protocolType;

    public CustomProtocolSetup(Type transceiverType, Type protocolType, IServiceCollection services)
        : base(transceiverType, services)
    {
        _protocolType = protocolType;
    }

    public override void SetupClient()
    {
        base.SetupClient();
        _ = Services.AddSingleton(typeof(ITransceiverProtocol), _protocolType);
    }

    public override void SetupServer(bool serverOnly)
    {
        base.SetupServer(serverOnly);
        _ = Services.AddSingleton(typeof(ITransceiverProtocol), _protocolType);
    }
}