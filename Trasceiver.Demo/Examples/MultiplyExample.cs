// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using System.Diagnostics;
using System.Threading.RateLimiting;
using Transceiver.Demo.Requests;

namespace Transceiver.Demo;

public sealed class MultiplyExample(ITransceiver<GeneratedRequests.Requests.MultiplyRequest, GeneratedRequests.Responses.MultiplyResponse> transceiver)
    : Example<GeneratedRequests.Requests.MultiplyRequest, GeneratedRequests.Responses.MultiplyResponse>(transceiver)
{
    public override GeneratedRequests.Requests.MultiplyRequest CreateRequest()
    {
        return new GeneratedRequests.Requests.MultiplyRequest
        {
            A = Random.Shared.Next(),
            B = Random.Shared.Next()
        };
    }

    public override Task<GeneratedRequests.Responses.MultiplyResponse> ProcessRequest(GeneratedRequests.Requests.MultiplyRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new GeneratedRequests.Responses.MultiplyResponse
        {
            Result = request.A * request.B
        });
    }

    public override void ProcessResponse(RateLimitLease lease, GeneratedRequests.Requests.MultiplyRequest request, GeneratedRequests.Responses.MultiplyResponse response)
    {
        Debug.Assert(response.Result == request.A * request.B, "Multiplication is incorrect");
        if (lease.IsAcquired)
        {
            Console.WriteLine($"Multiplication: {request.A} * {request.B} = {response.Result}");
        }
    }
}