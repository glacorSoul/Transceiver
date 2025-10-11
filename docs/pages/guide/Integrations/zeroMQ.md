# ZeroMQ

`Transceiver` integrates with `ZeroMQ`. You need to install the [Transceiver.ZeroMQ](https://www.nuget.org/packages/Transceiver.ZeroMQ) plugin.

```bash
dotnet add package Transceiver.ZeroMQ
```

This is an example on how you can set up `Transceiver` with ZeroMQ. This example can be viewed on [git](https://github.com/glacorSoul/Transceiver/blob/main/Trasceiver.Demo/Options/ZeroMqOptions.cs)

```csharp
public override void Run(IServiceCollection services, CancellationToken cancellationToken)
{
    _ = services.AddTransceiverZeroMq((setup) =>
    {
        setup.SetupServer(false);
        setup.SetupClient();
    }, new(IPAddress.Loopback, Port), typeof(Program).Assembly);
    RunSamples(services, CancellationToken.None);
}
```