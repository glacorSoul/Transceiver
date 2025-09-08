// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Reflection;
using Transceiver.Tests;

[assembly: AssemblyFixture(typeof(GlobalFixture))]

namespace Transceiver.Tests;

[Collection(nameof(GlobalCollectionSetup))]
public class GlobalCollectionSetup : ICollectionFixture<GlobalFixture>;

public class GlobalFixture
{
    static GlobalFixture()
    {
        Assembly assembly = typeof(GlobalFixture).Assembly;
        IServiceCollection services = new ServiceCollection();
        _ = services.AddLogging().AddTransceiver(config =>
        {
            if (config.Type == typeof(ITransceiver<UdpSumRequest, UdpSumResponse>))
            {
                ITransceiverSetup setup = config.ConfigureUdp(new(IPAddress.Loopback, 8889));
                setup.SetupServer(false);
                setup.SetupClient();
            }
            else if (config.Type == typeof(ITransceiver<TcpSumRequest, TcpSumResponse>))
            {
                ITransceiverSetup setup = config.ConfigureTcp(new(IPAddress.Loopback, 8889));
                setup.SetupServer(false);
                setup.SetupClient();
            }
            else
            {
                ITransceiverSetup setup = config.ConfigureDirectProtocol();
                setup.SetupServer(false);
                setup.SetupClient();
            }
        }, assembly);
        ServiceProvider provider = services.BuildServiceProvider();
        provider.ConfigureTransceiverProvider(assembly);
        Provider = provider;
    }

    public GlobalFixture()
    {
        Serviceprovider = Provider;
    }

    public static ServiceProvider Provider { get; }
    public ServiceProvider Serviceprovider { get; }
}

public class TestFixture
{
    public TestFixture()
    {
        Provider = GlobalFixture.Provider;
    }

    public ServiceProvider Provider { get; }
}

[Collection(nameof(TestCollectionSetup))]
public class TestCollectionSetup : ICollectionFixture<TestFixture>;