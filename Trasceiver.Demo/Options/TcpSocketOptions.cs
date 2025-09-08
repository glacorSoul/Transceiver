// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using Transceiver.Demo.Options;

namespace Transceiver.Demo;

[Verb(name: "tcp", isDefault: false)]
public sealed class TcpSocketOptions : BaseOptions
{
    [Option('p', "port", Required = false, HelpText = "Server port to connect to.", Default = 11111)]
    public int ServerPort { get; set; } = 11111;

    public override void Run(IServiceCollection services, CancellationToken cancellationToken)
    {
        _ = services.AddTransceiver((config) =>
        {
            ITransceiverSetup setup = config.ConfigureTcp(new(IPAddress.Loopback, ServerPort));
            setup.SetupServer(false);
            setup.SetupClient();
        }, typeof(Program).Assembly);
        RunSamples(services, cancellationToken);
    }
}