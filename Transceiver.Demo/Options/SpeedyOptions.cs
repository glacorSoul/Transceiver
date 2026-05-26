// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;

namespace Transceiver.Demo.Options;

[Verb(name: "speedy", isDefault: false)]
internal class SpeedyOptions : BaseOptions
{
    public override void Run(IServiceCollection services, CancellationToken cancellationToken)
    {
        _ = services.AddTransceiver(config =>
        {
            ITransceiverSetup setup = config.ConfigureDirectProtocol();
            setup.SetupServer(false);
            setup.SetupClient();
        }, typeof(Program).Assembly);
        services = services
            .AddSingleton<SumExample>()
            .AddSingleton<MultiplyExample>();
        ServiceProvider provider = services.BuildServiceProvider();
        provider.ConfigureTransceiverProvider(typeof(BaseOptions).Assembly);

        ITransceiver<SumRequest, SumResponse> transceiver = provider.GetRequiredService<ITransceiver<SumRequest, SumResponse>>();
        int n = 0;
        long now = Stopwatch.GetTimestamp();
        while (n < 1_000_000)
        {
            _ = transceiver.TransceiveOnceAsync(new SumRequest { A = n, B = n }, cancellationToken).GetAwaiter().GetResult();
            n++;
        }
        TimeSpan elapsed = Stopwatch.GetElapsedTime(now);
        Console.WriteLine($"Transceived 1,000,000 requests in {elapsed.TotalSeconds} seconds.");
        Console.WriteLine(elapsed.ToString());
        Console.WriteLine((n / elapsed.TotalSeconds) + " transceives per second.");
    }
}
