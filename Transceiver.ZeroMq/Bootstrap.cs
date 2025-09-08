// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Reflection;

namespace Transceiver.ZeroMq;

public static class BootStrap
{
    public static IServiceCollection AddTransceiverZeroMq(this IServiceCollection services, Action<ITransceiverSetup> doSetup, IPEndPoint endpoint, Assembly assembly)
    {
        return services.AddTransceiver(cfg =>
        {
            ITransceiverSetup setup = cfg.ConfigureCustomProtocol(typeof(ZeroMqProtocol));
            doSetup(setup);
            _ = services.AddTransient(sp => endpoint);
        }, assembly);
    }
}