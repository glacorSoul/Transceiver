// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using Transceiver.Demo.Options;

namespace Transceiver.Demo;

[Verb(name: "ssl", isDefault: false)]
public sealed class SslOptions : BaseOptions
{
    [Option('c', "cert", Required = true, HelpText = "Certificate thumbprint")]
    public string CertificateThumbprint { get; set; } = string.Empty;

    [Option('p', "port", Required = false, HelpText = "Server Port", Default = 10001)]
    public int ServerPort { get; set; } = 10001;

    public override void Run(IServiceCollection services, CancellationToken cancellationToken)
    {
        _ = services.AddTransceiver((config) =>
        {
            ITransceiverSetup setup = config.ConfigureSsl(new(IPAddress.Loopback, ServerPort));
            setup.SetupServer(false);
            setup.SetupClient();
        }, typeof(Program).Assembly);
        _ = services.Configure<TransceiverConfiguration>(cfg =>
        {
            cfg.CertificateThumbprint = CertificateThumbprint;
        });
        RunSamples(services, cancellationToken);
    }
}