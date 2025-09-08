// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Reflection;
using System.Text.Json;
using Transceiver.Requests;

namespace Transceiver.ServiceDiscoverer;

internal static class Program
{
    public static async Task<ServiceDiscoveryResponse> GetService(string url)
    {
        using CancellationTokenSource cts = new();
        cts.CancelAfter(TimeSpan.FromSeconds(20));
        Uri uri = new(url);
        ServiceCollection services = new();
        _ = services.AddTransceiver(config =>
        {
            string scheme = uri.Scheme.ToLowerInvariant();
            IPEndPoint endpoint = new(IPAddress.Parse(uri.Host), uri.Port);
            ITransceiverSetup setup = scheme switch
            {
                "tcp" => config.ConfigureTcp(endpoint),
                "udp" => config.ConfigureUdp(endpoint),
                "ssl" => config.ConfigureSsl(endpoint),
                _ => throw new NotSupportedException($"The scheme '{uri.Scheme}' is not supported.")
            };
            setup.SetupClient();
        }, Assembly.GetExecutingAssembly());
        _ = services.AddLogging();
        ServiceProvider provider = services.BuildServiceProvider();
        provider.ConfigureTransceiverProvider(Assembly.GetExecutingAssembly());

        ITransceiver<ServiceDiscoveryRequest, ServiceDiscoveryResponse> transceiver
            = provider.GetRequiredService<ITransceiver<ServiceDiscoveryRequest, ServiceDiscoveryResponse>>();
        ServiceDiscoveryResponse serviceDiscovery = await transceiver.TransceiveOnceAsync(new(), cts.Token);
        return serviceDiscovery;
    }

    public static async Task Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.WriteLine("Usage: ServiceDiscoverer <url>");
            return;
        }
        ServiceDiscoveryResponse serviceDiscovery = await GetService(args[0]);

        JsonSerializerOptions options = new(JsonSerializerDefaults.Web)
        {
            WriteIndented = true
        };
        await File.WriteAllTextAsync("transceiverServices.json", JsonSerializer.Serialize(serviceDiscovery, options));
    }
}