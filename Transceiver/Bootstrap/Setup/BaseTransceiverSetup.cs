// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;
using Transceiver.Requests;

namespace Transceiver;

public class BaseTransceiverSetup : ITransceiverSetup
{
    public BaseTransceiverSetup(Type transceiverType, IServiceCollection services)
    {
        Type[] genericArguments = transceiverType.GetGenericArguments();
        TransceiverType = typeof(Transceiver<,>).MakeGenericType(genericArguments);
        PipelineType = typeof(IPipelineProcessor<,>).MakeGenericType(genericArguments);
        ProcessorType = typeof(IProcessor<,>).MakeGenericType(genericArguments);
        CompositePipelineType = typeof(CompositePipelineProcessor<,>).MakeGenericType(genericArguments);
        Type = transceiverType;
        Services = services;
        _ = services.AddSingleton<CorrelatedMessageProcessor>()
            .AddSingleton<IMessageProcessor, CorrelatedMessageProcessor>(provider => provider.GetRequiredService<CorrelatedMessageProcessor>())
            .AddTransient(CompositePipelineType, provider =>
            {
                IEnumerable<object?> processors = provider.GetServices(PipelineType);
                return Activator.CreateInstance(CompositePipelineType, processors)!;
            });
    }

    protected Type CompositePipelineType { get; }
    protected Type PipelineType { get; }
    protected Type ProcessorType { get; }
    protected IServiceCollection Services { get; }
    protected Type TransceiverType { get; }
    protected Type Type { get; }

    public virtual void SetupClient()
    {
        Services.TryAddSingleton<TypeIdAssigner>();
        Services.TryAddSingleton(Type, provider =>
        {
            object pipeline = provider.GetRequiredService(CompositePipelineType);
            object protocol = provider.GetRequiredService<ITransceiverProtocol>();
            object transceiver = Activator.CreateInstance(TransceiverType, protocol, pipeline)!;
            if (transceiver is ITransceiver<ServiceDiscoveryRequest, ServiceDiscoveryResponse> serviceDiscoveryTransceiver)
            {
                _ = serviceDiscoveryTransceiver.TransceiveOnceAsync(new(), CancellationToken.None)
                    .ContinueWith(response =>
                    {
                        TypeIdAssigner idAssigner = provider.GetRequiredService<TypeIdAssigner>();
                        idAssigner.UpdateFrom(response.Result.TypeIdAssigner);
                    });
            }
            return transceiver;
        });
    }

    public virtual void SetupServer(bool serverOnly)
    {
        _ = Services.AddSingleton(Type, provider =>
        {
            object pipeline = provider.GetRequiredService(CompositePipelineType);
            object protocol = provider.GetRequiredService<ITransceiverProtocol>();
            object transceiver = Activator.CreateInstance(TransceiverType, protocol, pipeline)!;
            object processor = transceiver;
            if (!ProcessorType.IsInstanceOfType(transceiver))
            {
                processor = provider.GetRequiredService(ProcessorType);
            }

            const string startAsyncMethod = nameof(ITransceiver<string, string>.StartProcessingRequestsAsync);
            Type[] startAsyncArguments = [ProcessorType, typeof(CancellationToken)];
            MethodInfo start = transceiver.GetType().GetMethod(startAsyncMethod, startAsyncArguments)!;
            _ = (Task)start.Invoke(transceiver, [processor, CancellationToken.None])!;
            return transceiver;
        });
        _ = Services.AddSingleton(provider =>
        {
            return TypeIdAssigner.CreateServerAssigner();
        });
    }
}