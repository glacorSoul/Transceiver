# Kafka

`Transceiver` integrates with `Google pub/sub service`. You need to install the [Transceiver.Kafka](https://www.nuget.org/packages/Transceiver.Kafka) plugin.

```bash
dotnet add package Transceiver.Kafka
```

This is an example on how you can set up `Transceiver` with Kafka. This example can be viewed on [git](https://github.com/glacorSoul/Transceiver/blob/main/Trasceiver.Demo/Options/KafkaOptions.cs)

```csharp
public void Run(IServiceCollection services, CancellationToken cancellationToken)
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
```