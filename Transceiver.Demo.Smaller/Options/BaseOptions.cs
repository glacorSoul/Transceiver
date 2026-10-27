// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using System.Net;
using Microsoft.Extensions.DependencyInjection;

namespace Transceiver.Demo.Smaller.Options;

public abstract class BaseOptions
{
    public abstract void Run(IServiceCollection services, CancellationToken cancellationToken);

    protected static string GetLocalIPAddress()
    {
        IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
        IPAddress? ipAddress = host.AddressList
            .FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            ?? throw new InvalidOperationException("No IPv4 address found for this machine.");
        return ipAddress.ToString();
    }

    protected static void RunSamples(IServiceCollection services, CancellationToken cancellationToken)
    {
        services = services
            .AddSingleton<SumExample>()
            .AddSingleton<MultiplyExample>();
        ServiceProvider provider = services.BuildServiceProvider();
        provider.ConfigureTransceiverProvider();

        SumExample sumExample = provider.GetRequiredService<SumExample>();
        _ = ThreadPool.QueueUserWorkItem((ctx) => _ = sumExample.Execute(cancellationToken));

        MultiplyExample multiplyExample = provider.GetRequiredService<MultiplyExample>();
        _ = ThreadPool.QueueUserWorkItem((ctx) => _ = multiplyExample.Execute(cancellationToken));
    }
}