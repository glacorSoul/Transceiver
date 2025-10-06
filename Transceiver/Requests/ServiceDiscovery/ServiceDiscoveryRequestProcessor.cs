// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using System.Reflection;

namespace Transceiver.Requests;

public class ServiceDiscoveryRequestProcessor : IProcessor<ServiceDiscoveryRequestModel, ServiceDiscoveryResponseModel>
{
    internal ITransceiver<ServiceDiscoveryRequestModel, ServiceDiscoveryResponseModel> Transceiver { get; set; } = default!;

    public Task<ServiceDiscoveryResponseModel> ProcessRequest(ServiceDiscoveryRequestModel request, CancellationToken cancellationToken)
    {
        IEnumerable<Type> transceiverTypes = [..Assembly.GetExecutingAssembly().DiscoverType(typeof(ITransceiver<,>))
            .Concat(Assembly.GetEntryAssembly()!.DiscoverType(typeof(ITransceiver<,>)))];

        ServiceDiscoveryResponseModel result = new()
        {
            Services = transceiverTypes
            .Select(t => new ServiceDiscoveryModel
            {
                RequestName = t.GetGenericArguments()[0].ToShortPrettyString(),
                RequestProperties = Activator.CreateInstance(t.GetGenericArguments()[0])!.ToDictionary()!,
                ResponseName = t.GetGenericArguments()[1].ToShortPrettyString(),
                ResponseProperties = Activator.CreateInstance(t.GetGenericArguments()[1])!.ToDictionary()!,
            }),
            TypeIdAssigner = (TypeIdAssigner)BootStrap.ServiceProvider.GetService(typeof(TypeIdAssigner))!,
        };
        return Task.FromResult(result);
    }
}