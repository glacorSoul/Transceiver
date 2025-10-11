# Azure Queues

`Transceiver` integrates with `Azure Queues`. You need to install the [Transceiver.AzureQueue](https://www.nuget.org/packages/Transceiver.AzureQueue) plugin.

```bash
dotnet add package Transceiver.AzureQueue
```

This is an example on how you can set up `Transceiver` with Azure Queues. This example can be viewed on [git](https://github.com/glacorSoul/Transceiver/blob/main/Trasceiver.Demo/Options/AzureQueueOptions.cs)

```csharp
public void Run(IServiceCollection services, CancellationToken cancellationToken)
{
    string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    string path = Path.Combine(userProfile, ".azure");
    string connectionString = ConnectionString ?? File.ReadAllText(Path.Combine(path, "credentials"));

    _ = services.AddTransceiverAzureQueue(setup =>
    {
        setup.SetupServer(false);
        setup.SetupClient();
    }, name => new QueueClient(connectionString, name), typeof(Program).Assembly);
    RunSamples(services, CancellationToken.None);
}
```