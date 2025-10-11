# Discovery

Discovery in `Transceiver` can be divided in two categories. One type is `DI Discovery` other type is `Service Discovery`.

## Transceiver DI Discovery

When you configure `Transceiver` it scans the provided dll with two main types: `ITransceiver<TRequest, TResponse>` and `IProcessor<TRequest, TResponse>`.
In order for these types to be detected correctly you must have a Property, field or constructor with the `ITransceiver` type.
Transceiver will automatically configure the protocol, logger, configuration, pipeline and other types per each detected transceiver.
While it is possible to configure `Transceiver` with different protocols/integrations within the same assembly, it is strongly encouraged that such configuration
is done within different assemblies.

Transceiver will also call the method `StartProcessingRequestsAsync` while providing the respective processor. 
This way the server will start handling your requests automatically!

## Transceiver Service Discovery

Other type of discovery `Transceiver` has is `Service Discovery`. 
Service discovery is responsible for assigning an `Id` for request and response types.
It will be created a local file named `TypeIdMap.json` that contains information about the types and their respective ids.
In case of a server restart this file will be read again and the ids will be the same.
This information is automatically sent from the server to the client when the client connects.
