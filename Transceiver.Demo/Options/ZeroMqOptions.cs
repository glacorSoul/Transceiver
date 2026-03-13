// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using Transceiver.Demo.Options;
using Transceiver.ZeroMq;

namespace Transceiver.Demo;

[Verb(name: "zeroMq", isDefault: false)]
public sealed class ZeroMqOptions : BaseOptions
{
    [Option('p', "port", Required = false, HelpText = "ZeroMQ Port", Default = 9090)]
    public int Port { get; set; } = 9090;

    public override void Run(IServiceCollection services, CancellationToken cancellationToken)
    {
        _ = services.AddTransceiverZeroMq((setup) =>
        {
            setup.SetupServer(false);
            setup.SetupClient();
        }, new(IPAddress.Loopback, Port), typeof(Program).Assembly);
        RunSamples(services, CancellationToken.None);
    }
}