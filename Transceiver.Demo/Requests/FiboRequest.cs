// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using System.Runtime.CompilerServices;
using MessagePack;

namespace Transceiver.Demo.Requests;

[MessagePackObject]
public class FiboRequest
{
    [Key(0)]
    public int N { get; set; }
}

[MessagePackObject]
public class FiboResponse
{
    [Key(0)]
    public long Result { get; set; }
}

public class FiboProcessor : IProcessor<FiboRequest, FiboResponse>
{
    public Task<FiboResponse> ProcessRequestAsync(IClientRequest<FiboRequest, FiboResponse> request, CancellationToken cancellationToken)
    {
#pragma warning disable AsyncFixer03 // Fire-and-forget async-void methods or delegates
        _ = ThreadPool.QueueUserWorkItem(async (ctx) =>
        {
            await foreach (long fibo in FibonacciSequence(cancellationToken))
            {
                await request.SendResponseAsync(new FiboResponse { Result = fibo }, cancellationToken);
            }
        });
#pragma warning restore AsyncFixer03 // Fire-and-forget async-void methods or delegates
        return Task.FromResult(new FiboResponse() { Result = 0 });
    }

    private static async IAsyncEnumerable<long> FibonacciSequence([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        long a = 1;
        long b = 1;
        long c;
        yield return a;
        yield return b;
        while (!cancellationToken.IsCancellationRequested)
        {
            c = a + b;
            a = b;
            b = c;
            yield return c;
            await Task.Delay(500, cancellationToken);
        }
    }
}