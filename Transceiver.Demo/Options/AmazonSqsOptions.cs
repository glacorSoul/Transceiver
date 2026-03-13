// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using Amazon;
using Amazon.SQS;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Transceiver.AmazonSqs;
using Transceiver.Demo.Options;

namespace Transceiver.Demo;

[Verb(name: "amazonSqs", isDefault: false)]
public sealed class AmazonSqsOptions : BaseOptions
{
    public override void Run(IServiceCollection services, CancellationToken cancellationToken)
    {
        string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".aws");
        string[] lines = File.ReadAllLines(Path.Combine(path, "credentials"));
        string id = lines[1][lines[1].LastIndexOf(" ", StringComparison.InvariantCultureIgnoreCase)..].Trim();
        string secret = lines[2][lines[2].LastIndexOf(" ", StringComparison.InvariantCultureIgnoreCase)..].Trim();
        _ = services.AddTransceiverAmazonSqs(setup =>
        {
            setup.SetupServer(false);
            setup.SetupClient();
        }, () => new AmazonSQSClient(id, secret, RegionEndpoint.EUSouth1), typeof(Program).Assembly);
        RunSamples(services, cancellationToken);
    }
}