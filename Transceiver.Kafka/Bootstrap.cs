// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;

namespace Transceiver.Kafka;

public static class BootStrap
{
    public static IServiceCollection AddTransceiverKafka(this IServiceCollection services, Action<ITransceiverSetup> doSetup,
        AdminClientConfig adminConfig, ProducerConfig producerConfig, ConsumerConfig consumerConfig, Assembly assembly)
    {
        return services.AddTransceiver(cfg =>
        {
            ITransceiverSetup setup = cfg.ConfigureDelegateToMessageProcessor();
            doSetup(setup);
            _ = services.RemoveAll<IMessageProcessor>()
                .AddSingleton(producerConfig)
                .AddSingleton(consumerConfig)
                .AddSingleton(adminConfig)
                .AddSingleton<IMessageProcessor, KafkaMessageProcessor>();
        }, assembly);
    }
}