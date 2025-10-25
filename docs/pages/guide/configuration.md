# Transceiver Configuration

Transceiver configuration can be setup with depedency Injection, providing an instance of [TransceiverConfiguration](https://github.com/glacorSoul/Transceiver/blob/main/Transceiver/TransceiverConfiguration.cs)

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

The description of the configuration properties are as follows:

| Property                   | Description                                                                                                                                      |
|----------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------|
| CertificateThumbprint      | Allows to configure a certificate from the certification store Machine/Personal with the specified thumbprint.                                   |
| OptimizeHelloSerialization | Allows to configure messages with smaller traffic. This configuration is experimental.                                                           |
| RequestTimeout             | Configures request timeout.                                                                                                                      |
| NRetries                   | Number of attempts transceiver will make when sending request/response to server and client.                                                     |
| DelayBetweenRetriesMs      | Delay between attemps.                                                                                                                           |
