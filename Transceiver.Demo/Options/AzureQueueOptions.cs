// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using Azure.Storage.Queues;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Transceiver.AzureQueue;
using Transceiver.Demo.Options;

namespace Transceiver.Demo;

[Verb(name: "azureQueue", isDefault: false)]
public sealed class AzureQueueOptions : BaseOptions
{
    [Option('c', "connectionString", Required = false, HelpText = "Azure Storage Account connection string.")]
    public string? ConnectionString { get; set; }

    public override void Run(IServiceCollection services, CancellationToken cancellationToken)
    {
        string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".azure");
        string connectionString = ConnectionString ?? File.ReadAllText(Path.Combine(path, "credentials"));

        _ = services.AddTransceiverAzureQueue(setup =>
        {
            setup.SetupServer(false);
            setup.SetupClient();
        }, name => new QueueClient(connectionString, name), typeof(Program).Assembly);
        RunSamples(services, CancellationToken.None);
    }
}