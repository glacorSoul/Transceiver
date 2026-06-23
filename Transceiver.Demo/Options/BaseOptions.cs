// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using Microsoft.Extensions.DependencyInjection;
using Transceiver.Demo.Examples;

namespace Transceiver.Demo.Options;

public abstract class BaseOptions
{
    public abstract void Run(IServiceCollection services, CancellationToken cancellationToken);

    protected static void RunSamples(IServiceCollection services, CancellationToken cancellationToken)
    {
        services = services
            .AddSingleton<SumExample>()
            .AddSingleton<MultiplyExample>()
            .AddSingleton<FiboExample>();
        ServiceProvider provider = services.BuildServiceProvider();
        provider.ConfigureTransceiverProvider(typeof(BaseOptions).Assembly);

        SumExample sumExample = provider.GetRequiredService<SumExample>();
        _ = ThreadPool.QueueUserWorkItem((ctx) => _ = sumExample.Execute(cancellationToken));

        MultiplyExample multiplyExample = provider.GetRequiredService<MultiplyExample>();
        _ = ThreadPool.QueueUserWorkItem((ctx) => _ = multiplyExample.Execute(cancellationToken));

        FiboExample fiboExample = provider.GetRequiredService<FiboExample>();
        _ = ThreadPool.QueueUserWorkItem((ctx) => _ = fiboExample.Execute(cancellationToken));
    }
}