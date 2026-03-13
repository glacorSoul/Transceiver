// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Transceiver.Demo.Options;
using Transceiver.GooglePubSub;

namespace Transceiver.Demo;

[Verb(name: "googlePubSub", isDefault: false)]
public sealed class GooglePubSubOptions : BaseOptions
{
    [Option('p', "projectId", Required = true, HelpText = "Google Cloud Project Id.")]
    public string ProjectId { get; set; } = null!;

    public override void Run(IServiceCollection services, CancellationToken cancellationToken)
    {
        _ = services.AddTransceiverGooglePubSub(setup =>
        {
            setup.SetupServer(false);
            setup.SetupClient();
        }, new GooglePubSubConfig
        {
            ProjectId = ProjectId
        }, typeof(Program).Assembly);
        RunSamples(services, cancellationToken);
    }
}