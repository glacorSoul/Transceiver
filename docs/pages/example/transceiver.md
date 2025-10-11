# Setting up Transceiver with DI

Transceiver has been built with DI in mind, and provides methods that allows you to configure its depedencies. 
You can configure Transceiver in the following way:

```csharp
public static ServiceProvider ConfigureTransceiverDI()
{
    IServiceCollection services = new ServiceCollection();
    Assembly assembly = this.GetType().Assembly;
    _ = services.AddLogging().AddTransceiver(config =>
    {
        ITransceiverSetup setup = config.ConfigureTcp(new(IPAddress.Loopback, 8889));
        setup.SetupServer(false);
        setup.SetupClient();
    }, assembly);
    ServiceProvider provider = services.BuildServiceProvider();
    provider.ConfigureTransceiverProvider(assembly);
}
```

## Handle Requests with Transceiver

In order to handle requests in your server you need to create an `IProcessor` 

```csharp
public class SumProcessor : IProcessor<SumRequest, SumResponse>
{
    public Task<SumResponse> ProcessRequest(TcpSumRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new SumResponse(request.A + request.B));
    }
}
```

## Request/Response pattern with Transceiver

You can then send requests to the processor using `Transceiver`. 
In order for your transceivers to be correctly detected you can configure them in your constructor, as a private field or as a property.

```csharp
public class Program
{
    //You need to have a reference to the Transceiver in some capacity.
    private ITransceiver<SumRequest, SumResponse> _transceiver;
    public static async Task Main(string[])
    {
        ServiceProvider provider = ConfigureTransceiverDI();
        _transceiver = provider.GetService<ITransceiver<SumRequest, SumResponse>>()!; 
        SumResponse sumResponse = await _transceiver.TransceiveOnceAsync(request, CancellationToken.None);
        Console.WriteLine(sumResponse.Result);
    }
}
```