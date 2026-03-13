// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Transceiver.Demo.Options;
using Transceiver.Websockets;

namespace Transceiver.Demo;

[Verb(name: "wss", isDefault: false)]
public sealed class WebsocketsOptions : BaseOptions
{
    [Option('u', "uri", Required = false, HelpText = "Server port to connect to.", Default = "wss://localhost:8516")]
    public string Uri { get; set; } = default!;
    [Option('c', "cert", Required = true, HelpText = "Certificate thumbprint to use with WebSockets")]
    public string CertificateThumbprint { get; set; } = default!;

    public override void Run(IServiceCollection services, CancellationToken cancellationToken)
    {
        _ = services.AddTransceiverWebSockets(setup =>
        {
            setup.SetupServer(false);
            setup.SetupClient();
        }, new Uri(Uri), typeof(Program).Assembly);
        _ = services.Configure<TransceiverConfiguration>(cfg =>
        {
            cfg.CertificateThumbprint = CertificateThumbprint;
        });
        RunSamples(services, cancellationToken);
    }
}