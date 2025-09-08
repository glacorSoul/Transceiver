// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using CommandLine;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Transceiver.Demo.Options;
using Transceiver.Kafka;

namespace Transceiver.Demo;

[Verb(name: "kafka", isDefault: false)]
public sealed class KafkaOptions : BaseOptions
{
    public override void Run(IServiceCollection services, CancellationToken cancellationToken)
    {
        AdminClientConfig adminConfig = new()
        {
            BootstrapServers = "localhost:9092",
            ClientId = "transceiver-admin"
        };
        ProducerConfig producerConfig = new()
        {
            BootstrapServers = "localhost:9092",
            ClientId = "transceiver-producer",
            Acks = Acks.All,
            EnableIdempotence = true,
            MessageSendMaxRetries = 3,
            LingerMs = 5,
            BatchSize = 32 * 1024
        };
        ConsumerConfig consumerConfig = new()
        {
            BootstrapServers = "localhost:9092",
            ClientId = "transceiver-consumer",
            GroupId = "transceiver-group",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true,
            AllowAutoCreateTopics = true
        };
        _ = services.AddTransceiverKafka(setup =>
        {
            setup.SetupServer(false);
            setup.SetupClient();
        }, adminConfig, producerConfig, consumerConfig, typeof(Program).Assembly);
        RunSamples(services, cancellationToken);
    }
}