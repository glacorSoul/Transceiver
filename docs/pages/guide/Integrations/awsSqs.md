# Amazon SQS

`Transceiver` integrates with `AmazonSQS`. You need to install the [Transceiver.AmazonSqs](https://www.nuget.org/packages/Transceiver.AmazonSqs) plugin.

```bash
dotnet add package Transceiver.AmazonSqs
```

This is an example on how you can set up `Transceiver` with AmazonSqs. This example can be viewed on [git](https://github.com/glacorSoul/Transceiver/blob/main/Trasceiver.Demo/Options/AmazonSqsOptions.cs)

```csharp
public void Run(IServiceCollection services, CancellationToken cancellationToken)
{
    string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    string path = Path.Combine(userProfile, ".aws");
    string[] lines = File.ReadAllLines(Path.Combine(path, "credentials"));
    string id = lines[1][lines[1]
        .LastIndexOf(" ", StringComparison.InvariantCultureIgnoreCase)..]
        .Trim();
    string secret = lines[2][lines[2]
        .LastIndexOf(" ", StringComparison.InvariantCultureIgnoreCase)..]
        .Trim();
    _ = services.AddTransceiverAmazonSqs(setup =>
    {
        setup.SetupServer(false);
        setup.SetupClient();
    }, () => new AmazonSQSClient(id, secret, RegionEndpoint.EUSouth1), typeof(Program).Assembly);
    RunSamples(services, cancellationToken);
}
```