# Bulit In Integrations

`Transceiver` integrates with TCP, UDP and SSL protocols builtin. You need to install the [Transceiver](https://www.nuget.org/packages/Transceiver) package.

```bash
dotnet add package Transceiver
```

## Udp

This is an example on how you can set up `Transceiver` with Udp. This example can be viewed on [git](https://github.com/glacorSoul/Transceiver/blob/main/Trasceiver.Demo/Options/UdpSocketOptions.cs)

```csharp
public void Run(IServiceCollection services, CancellationToken cancellationToken)
{
    _ = services.AddTransceiver((config) =>
    {
        ITransceiverSetup setup = config.ConfigureUdp(new(IPAddress.Loopback, ServerPort));
        setup.SetupServer(false);
        setup.SetupClient();
    }, typeof(Program).Assembly);
    RunSamples(services, cancellationToken);
}
```

## Tcp

This is an example on how you can set up `Transceiver` with Tcp. This example can be viewed on [git](https://github.com/glacorSoul/Transceiver/blob/main/Trasceiver.Demo/Options/TcpSocketOptions.cs)

```csharp
public void Run(IServiceCollection services, CancellationToken cancellationToken)
{
    _ = services.AddTransceiver((config) =>
    {
        ITransceiverSetup setup = config.ConfigureTcp(new(IPAddress.Loopback, ServerPort));
        setup.SetupServer(false);
        setup.SetupClient();
    }, typeof(Program).Assembly);
    RunSamples(services, cancellationToken);
}
```

## Ssl

This is an example on how you can set up `Transceiver` with Ssl. This example can be viewed on [git](https://github.com/glacorSoul/Transceiver/blob/main/Trasceiver.Demo/Options/SslOptions.cs)

```csharp
public void Run(IServiceCollection services, CancellationToken cancellationToken)
{
    _ = services.AddTransceiver((config) =>
    {
        ITransceiverSetup setup = config.ConfigureSsl(new(IPAddress.Loopback, ServerPort));
        setup.SetupServer(false);
        setup.SetupClient();
    }, typeof(Program).Assembly);
    _ = services.Configure<TransceiverConfiguration>(cfg =>
    {
        cfg.CertificateThumbprint = CertificateThumbprint;
    });
    RunSamples(services, cancellationToken);
}
```