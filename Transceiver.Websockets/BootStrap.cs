// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using System.Reflection;

namespace Transceiver.Websockets;

public static class BootStrap
{
    public static IServiceCollection AddTransceiverWebSockets(this IServiceCollection services, Action<ITransceiverSetup> doSetup, Uri serverEndpoint, Assembly assembly)
    {
        _ = services.AddSingleton<WebsocketsProtocol>();
        _ = services.AddTransceiver(cfg =>
        {
            WebsocketsSetup setup = new(cfg.Type, cfg.Services, serverEndpoint);
            doSetup(setup);
        }, assembly);

        return services;
    }
}