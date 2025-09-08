// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using Amazon.SQS;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;

namespace Transceiver.AmazonSqs;

public static class BootStrap
{
    public static IServiceCollection AddTransceiverAmazonSqs(this IServiceCollection services, Action<ITransceiverSetup> doSetup, Func<AmazonSQSClient> sqsClientFactory, Assembly assembly)
    {
        return services.AddTransceiver(cfg =>
        {
            ITransceiverSetup setup = cfg.ConfigureDelegateToMessageProcessor();
            doSetup(setup);
            _ = services.RemoveAll<IMessageProcessor>()
                .AddSingleton(sqsClientFactory)
                .AddSingleton<IMessageProcessor, AmazonSqsMessageProcessor>();
        }, assembly);
    }
}