# RabbitMQ

`Transceiver` integrates with `RabbitMQ`. You need to install the [Transceiver.RabbitMq](https://www.nuget.org/packages/Transceiver.RabbitMq) plugin.

```bash
dotnet add package Transceiver.RabbitMq
```

This is an example on how you can set up `Transceiver` with RabbitMQ. This example can be viewed on [git](https://github.com/glacorSoul/Transceiver/blob/main/Trasceiver.Demo/Options/RabbitMqOptions.cs)

```csharp
public void Run(IServiceCollection services, CancellationToken cancellationToken)
{
    _ = services.AddTransceiverRabbitMq(setup =>
    {
        setup.SetupServer(false);
        setup.SetupClient();
    }, new RabbitMQ.Client.ConnectionFactory
    {
        Port = Port,
        HostName = HostName,
        UserName = User,
        Password = Password,
        ConsumerDispatchConcurrency = 4
    }, typeof(Program).Assembly);
    RunSamples(services, cancellationToken);
}
```