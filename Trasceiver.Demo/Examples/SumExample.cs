// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using System.Diagnostics;
using System.Threading.RateLimiting;

namespace Transceiver.Demo;

public sealed class SumExample(ITransceiver<SumRequest, SumResponse> transceiver)
    : Example<SumRequest, SumResponse>(transceiver)
{
    public override SumRequest CreateRequest()
    {
        return new SumRequest(Random.Shared.Next(), Random.Shared.Next());
    }

    public override Task<SumResponse> ProcessRequest(SumRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new SumResponse(request.A + request.B));
    }

    public override void ProcessResponse(RateLimitLease lease, SumRequest request, SumResponse response)
    {
        Debug.Assert(response.Result == request.A + request.B, "Sum is incorrect");
        if (lease.IsAcquired)
        {
            Console.WriteLine($"Sum: {request.A} + {request.B} = {response.Result}");
        }
    }
}