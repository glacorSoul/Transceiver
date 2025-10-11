# Google Pub/Sub

`Transceiver` integrates with `Google pub/sub service`. You need to install the [Transceiver.GooglePubSub](https://www.nuget.org/packages/Transceiver.GooglePubSub) plugin.

```bash
dotnet add package Transceiver.GooglePubSub
```

This is an example on how you can set up Google pub/sub. This example can be viewed on [git](https://github.com/glacorSoul/Transceiver/blob/main/Trasceiver.Demo/Options/GooglePubSubOptions.cs)

```csharp
public void Run(IServiceCollection services, CancellationToken cancellationToken)
{
    _ = services.AddTransceiverGooglePubSub(setup =>
    {
        setup.SetupServer(false);
        setup.SetupClient();
    }, new GooglePubSubConfig
    {
        ProjectId = ProjectId
    }, typeof(Program).Assembly);
    RunSamples(services, cancellationToken);
}
```