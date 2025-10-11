# Code Generation

`Transceiver` allows to generate code for requests and response models from a server, so you do not have to share Dlls!
In order to use code generation you can install the package [Transceiver.Generator](https://www.nuget.org/packages/Transceiver.Generator).

```bash
dotnet add package Transceiver.Generator
```

Code generation generates the models from a json specification file. This file can be retrieved using [Transceiver.ServiceDiscoverer](https://github.com/glacorSoul/Transceiver/tree/main/Transceiver.ServiceDiscoverer). 
If the property is a Dictionary property it must be prefixed by `%` character. 
At the moment the only Dictionary type that `Transceiver` is able to generate is `Dictionary<string, object>`.

You can then use the `ServiceDiscoveryAttribute` on a partial implemented class.

```csharp
[ServiceDiscovery("transceiverServices.json")]
public partial class GeneratedRequests
{
}
```
