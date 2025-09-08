// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;

namespace Transceiver.DomainSockets;

public static class BootStrap
{
    public static IServiceCollection AddTransceiverDomainSockets(this IServiceCollection services, Action<ITransceiverSetup> doSetup, Assembly assembly)
    {
        _ = services.AddTransceiver(cfg =>
        {
            ITransceiverSetup setup = cfg.ConfigureUnknownSocket();
            doSetup(setup);
        }, assembly);

        _ = services.AddSingleton<ISocketFactory, DomainSocketFactory>();

        return services;
    }
}