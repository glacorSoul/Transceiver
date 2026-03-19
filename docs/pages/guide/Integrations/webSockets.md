# Websockets

`Transceiver` allows communication with Websockets protocol. You need to install the [Transceiver.Websockets](https://www.nuget.org/packages/Transceiver.Websockets) plugin.

```bash
dotnet add package Transceiver.Websockets
```

This is an example on how you can set up `Transceiver` with Websockets. This example can be viewed on [git](https://github.com/glacorSoul/Transceiver/blob/main/Transceiver.Demo/Options/WebsocketsOptions.cs)

```csharp
public override void Run(IServiceCollection services, CancellationToken cancellationToken)
{
	_ = services.AddTransceiverWebSockets(setup =>
	{
		setup.SetupServer(false);
		setup.SetupClient();
	}, new Uri(Uri), typeof(Program).Assembly);
	_ = services.Configure<TransceiverConfiguration>(cfg =>
	{
		cfg.CertificateThumbprint = CertificateThumbprint;
	});
	RunSamples(services, cancellationToken);
}
```