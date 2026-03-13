// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using Transceiver.Demo.Smaller.Options;

namespace Transceiver.Demo.Smaller;

[Verb(name: "tcpServer", isDefault: false)]
public sealed class TcpServerOptions : BaseOptions
{
    [Option('s', "serverPort", Required = false, HelpText = "Server port to connect to.", Default = 11111)]
    public int ServerPort { get; set; } = 11111;

    public override void Run(IServiceCollection services, CancellationToken cancellationToken)
    {
        _ = services.AddTransceiver((config) =>
        {
            ITransceiverSetup setup = config.ConfigureTcp(new(IPAddress.Loopback, ServerPort));
            setup.SetupServer(true);
        }, typeof(Program).Assembly);
        RunServer(services);
    }

    private static void RunServer(IServiceCollection services)
    {
        ServiceProvider provider = services.BuildServiceProvider();
        provider.ConfigureTransceiverProvider(typeof(TcpServerOptions).Assembly);
    }
}