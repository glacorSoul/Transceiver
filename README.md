# Transceiver
Transceiver allows you to send messages In-process, between processes and between machines. 
Implementing, Request/Response and Publish/Subscribe patterns.
It also offers integration with relevant queuing Services like AmazonSQS, Azure Queues, Google Publish/Subscribe, RabbitMQ, Kafka and others.

# Setting up Transceiver
Transceiver has been built with DI in mind, and provides methods that allows you to configure its depedencies.
You can configure Transceiver in the following way:

```
    public override void Run(IServiceCollection services, CancellationToken cancellationToken)
    {
		Assembly assembly = this.GetType().Assembly;
        _ = services.AddTransceiver((config) =>
        {
            ITransceiverSetup setup = config.ConfigureTcp(new(IPAddress.Loopback, ServerPort));
            setup.SetupServer(false);
            setup.SetupClient();
        }, assembly);
        ServiceProvider provider = services.BuildServiceProvider();
        provider.ConfigureTransceiverProvider(assembly);
	}
```

# Using Transceiver
Transceiver implements Request/Response and Publish/Subscribe patterns.
It offers the methods `TransceiveOnceAsync` and `TransceiveMany` allowing to receive one, or multiple responses, per each request.
Please see [the Demo](Trasceiver.Demo/Examples/Example.cs) for an example on how to use it.