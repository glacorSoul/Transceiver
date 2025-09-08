// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Transceiver;

public static class BootStrap
{
    public static IServiceProvider ServiceProvider { get; private set; } = default!;

    public static IServiceCollection AddTransceiver(this IServiceCollection services,
        Action<TransceiverServicesConfiguration> config,
        Assembly currentAssembly,
        ISerializer? serializer = null)
    {
        if (serializer is null)
        {
            _ = services.AddSingleton<ISerializer, SerializerJson>();
        }
        else
        {
            _ = services.AddSingleton(serializer);
        }
        _ = services.AddSingleton<CorrelatedMessageProcessor>();
        _ = services.AddSingleton<TypeIdAssigner>();
        RegisterProcessors(services, currentAssembly);
        RegisterProcessors(services, typeof(BootStrap).Assembly);
        IEnumerable<Type> discoverableTransceivers = currentAssembly.DiscoverType(typeof(ITransceiver<,>))
            .Concat(typeof(BootStrap).Assembly.DiscoverType(typeof(ITransceiver<,>)));
        foreach (Type transceiverType in discoverableTransceivers)
        {
            config(new(services, transceiverType));
        }
        return services;
    }

    public static void ConfigureTransceiverProvider(this IServiceProvider serviceProvider, Assembly assembly)
    {
        ServiceProvider = serviceProvider;
        IEnumerable<Type> discoverableTransceivers = assembly.DiscoverType(typeof(ITransceiver<,>))
            .Concat(typeof(BootStrap).Assembly.DiscoverType(typeof(ITransceiver<,>)));
        foreach (Type transceiverType in discoverableTransceivers)
        {
            _ = ServiceProvider.GetRequiredService(transceiverType);
        }
    }

    private static void RegisterProcessors(IServiceCollection services, Assembly currentAssembly)
    {
        IEnumerable<Type> types = currentAssembly.DefinedTypes
            .Where(type => !type.IsAbstract && !type.IsInterface)
            .Select(t => t.AsType());

        types = types.AssignableFrom(typeof(IProcessor<,>));
        foreach (Type type in types)
        {
            IEnumerable<Type> processorInterface = type.GetInterfaces().AssignableFrom(typeof(IProcessor<,>));
            foreach (Type interfaceType in processorInterface)
            {
                _ = services.AddTransient(interfaceType, type);
            }
        }
    }
}