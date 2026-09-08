// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using System.Net;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Transceiver.Benchmarks;

internal static class DiagnosticSslTest
{
    public static async Task Test()
    {
        Console.WriteLine("=== Starting Diagnostic SSL Test ===\n");

        // Build transceiver with console logging
        ServiceCollection services = [];

        // Configure logging with Information level so we see our logs
        _ = services.AddLogging(builder =>
        {
            _ = builder.AddConsole();
            _ = builder.SetMinimumLevel(LogLevel.Information);
        });

        _ = services.AddTransceiver(t =>
        {
            ITransceiverSetup setup = t.ConfigureSsl(new(IPAddress.Loopback, 2124));
            setup.SetupServer(false, CancellationToken.None);
            setup.SetupClient(CancellationToken.None);
        }, Assembly.GetExecutingAssembly());

        _ = services.Configure<TransceiverConfiguration>(cfg =>
        {
            cfg.CertificateThumbprint = "34abfd4fc9e26a3315e1c398f51ebfc41ba0d553";
        });

        ServiceProvider serviceProvider = services.BuildServiceProvider();
        serviceProvider.ConfigureTransceiverProvider();

        ITransceiver<SslSumRequest, SslSumResponse> transceiver = serviceProvider.GetRequiredService<ITransceiver<SslSumRequest, SslSumResponse>>();

        // Test 1: Send first request
        Console.WriteLine("\n>>> Sending FIRST request...");
        SslSumRequest request1 = new() { A = 5, B = 3 };

        try
        {
            SslSumResponse response1 = await transceiver.TransceiveOnceAsync(request1, CancellationToken.None);
            Console.WriteLine($"<<< Received FIRST response: {response1.Result}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR on first request: {ex.Message}");
        }

        // Small delay
        await Task.Delay(1000);

        // Test 2: Send second request on SAME connection
        Console.WriteLine("\n>>> Sending SECOND request on SAME connection...");
        SslSumRequest request2 = new() { A = 10, B = 7 };

        CancellationTokenSource cts = new(); // 5 second timeout
        try
        {
            // This should hang if the bug is still there
            cts.CancelAfter(5000);
            SslSumResponse response2 = await transceiver.TransceiveOnceAsync(request2, cts.Token);
            Console.WriteLine($"<<< Received SECOND response: {response2.Result}");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("TIMEOUT! Second request hung (this is the bug)");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR on second request: {ex.Message}");
        }
        finally
        {
            cts.Dispose();
        }

        Console.WriteLine("\n=== Diagnostic Test Complete ===");
    }
}
