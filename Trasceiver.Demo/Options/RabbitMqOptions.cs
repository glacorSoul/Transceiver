// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Transceiver.Demo.Options;
using Transceiver.RabbitMq;

namespace Transceiver.Demo;

[Verb(name: "rabbitMq", isDefault: false)]
public sealed class RabbitMqOptions : BaseOptions
{
    [Option('h', "host", Required = false, HelpText = "RabbitMQ Host Name.", Default = "localhost")]
    public string HostName { get; set; } = "localhost";

    [Option('p', "pw", Required = false, HelpText = "RabbitMQ Password", Default = "transceiver")]
    public string Password { get; set; } = "transceiver";

    [Option('P', "port", Required = false, HelpText = "RabbitMQ Port", Default = 5672)]
    public int Port { get; set; } = 5672;

    [Option('u', "user", Required = false, HelpText = "RabbitMQ User Name", Default = "transceiver")]
    public string User { get; set; } = "transceiver";

    public override void Run(IServiceCollection services, CancellationToken cancellationToken)
    {
        _ = services.AddTransceiverRabbitMq(setup =>
        {
            setup.SetupServer(false);
            setup.SetupClient();
        }, new RabbitMQ.Client.ConnectionFactory
        {
            Port = Port,
            HostName = HostName,
            UserName = User,
            Password = Password,
            ConsumerDispatchConcurrency = 4
        }, typeof(Program).Assembly);
        RunSamples(services, cancellationToken);
    }
}