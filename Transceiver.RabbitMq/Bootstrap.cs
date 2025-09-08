// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RabbitMQ.Client;
using System.Reflection;

namespace Transceiver.RabbitMq;

public static class BootStrap
{
    public static IServiceCollection AddTransceiverRabbitMq(this IServiceCollection services, Action<ITransceiverSetup> doSetup, ConnectionFactory connectionFactory, Assembly assembly)
    {
        _ = services.AddTransceiver(cfg =>
        {
            ITransceiverSetup setup = cfg.ConfigureDelegateToMessageProcessor();
            doSetup(setup);
        }, assembly);
        _ = services.AddSingleton(sp => connectionFactory)
            .RemoveAll<IMessageProcessor>()
            .AddSingleton<IMessageProcessor, RabbitMqMessageProcessor>();
        return services;
    }
}