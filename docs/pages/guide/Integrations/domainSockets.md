# Domain Sockets

`Transceiver` integrates with `DomainSockets`. You need to install the [Transceiver.DomainSockets](https://www.nuget.org/packages/Transceiver.DomainSockets) plugin.
Domain Sockets is a protocol that allows you to exchange messages between two processes running on the same machine.

```bash
dotnet add package Transceiver.DomainSockets
```

This is an example on how you can set up `Transceiver` with DomainSockets. This example can be viewed on [git](https://github.com/glacorSoul/Transceiver/blob/main/Trasceiver.Demo/Options/DomainSocketsOptions.cs)

```csharp
public override void Run(IServiceCollection services, CancellationToken cancellationToken)
{
    _ = services.AddTransceiverDomainSockets(setup =>
    {
        setup.SetupServer(false);
        setup.SetupClient();
    }, typeof(Program).Assembly);
    RunSamples(services, cancellationToken);
}
```