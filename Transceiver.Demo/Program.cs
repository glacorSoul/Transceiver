// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Transceiver;
using Transceiver.Demo;
using Transceiver.Demo.Options;

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

static ParserResult<object> ParseArguments(string[] args, IServiceCollection services)
{
    ParserResult<object> result = Parser.Default.ParseArguments(
        args,
        typeof(AmazonSqsOptions),
        typeof(AzureQueueOptions),
        typeof(DirectOptions),
        typeof(DomainSocketsOptions),
        typeof(GooglePubSubOptions),
        typeof(KafkaOptions),
        typeof(RabbitMqOptions),
        typeof(SslOptions),
        typeof(TcpClientOptions),
        typeof(TcpServerOptions),
        typeof(TcpSocketOptions),
        typeof(UdpSocketOptions),
        typeof(WebsocketsOptions),
        typeof(WebsocketsClientOptions),
        typeof(WebsocketsSeverOptions),
        typeof(ZeroMqOptions)
    );
    if (result.Value is BaseOptions baseOptions)
    {
        baseOptions.Run(services, CancellationToken.None);
    }
    return result;
}