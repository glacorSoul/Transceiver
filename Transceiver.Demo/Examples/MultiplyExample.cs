// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using System.Diagnostics;
using System.Threading.RateLimiting;

namespace Transceiver.Demo;

public sealed class MultiplyExample(ITransceiver<MultiplyRequest, MultiplyResponse> transceiver)
    : Example<MultiplyRequest, MultiplyResponse>(transceiver)
{
    public override MultiplyRequest CreateRequest()
    {
        return new MultiplyRequest
        {
            A = Random.Shared.Next(),
            B = Random.Shared.Next()
        };
    }

    public override Task<MultiplyResponse> ProcessRequest(MultiplyRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new MultiplyResponse
        {
            Result = request.A * request.B
        });
    }

    public override void ProcessResponse(RateLimitLease lease, MultiplyRequest request, MultiplyResponse response)
    {
        Debug.Assert(response.Result == request.A * request.B, "Multiplication is incorrect");
        if (lease.IsAcquired)
        {
            Console.WriteLine($"Multiplication: {request.A} * {request.B} = {response.Result}");
        }
    }
}