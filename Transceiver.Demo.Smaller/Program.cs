// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Transceiver;
using Transceiver.Demo.Smaller;

await Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        _ = config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables();
    })
    .ConfigureLogging(logging =>
    {
        _ = logging.ClearProviders();
        _ = logging.AddConsole();
    })
    .ConfigureServices((context, services) =>
    {
        _ = services.Configure<TransceiverConfiguration>(context.Configuration.GetSection("Transceiver"));
    })
    .ConfigureServices((context, services) => _ = ParseArguments(args, services)).RunConsoleAsync();

ParserResult<object> ParseArguments(string[] args, IServiceCollection services)
{
    return Parser.Default.ParseArguments<
        TcpSocketOptions,
        TcpServerOptions,
        TcpClientOptions
    >(args)
        .WithParsed<TcpServerOptions>(options =>
        {
            options.Run(services, CancellationToken.None);
        })
        .WithParsed<TcpClientOptions>(options =>
        {
            options.Run(services, CancellationToken.None);
        });
}