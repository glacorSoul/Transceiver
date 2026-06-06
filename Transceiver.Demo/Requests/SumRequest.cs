// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using Transceiver.Demo.Requests;

namespace Transceiver.Demo;

public sealed class SumProcessor : IProcessor<SumRequest, SumResponse>
{
    public async Task<SumResponse> ProcessRequestAsync(ClientRequest<SumRequest, SumResponse> request, CancellationToken cancellationToken)
    {
        SumResponse response = new(request.Data.A + request.Data.B);
        await request.SendResponseAsync(response, cancellationToken);
        return response;
    }
}

public sealed class SumRequest
{
    public SumRequest()
    {
    }

    public SumRequest(int a, int b)
    {
        A = a;
        B = b;
    }

    public int A { get; set; }
    public int B { get; set; }
}

public sealed class SumResponse
{
    public SumResponse()
    {
    }

    public SumResponse(int result)
    {
        Result = result;
    }

    public int Result { get; set; }
}

public sealed class SumProcessor2 : IProcessor<GeneratedRequests.Requests.SumRequest, GeneratedRequests.Responses.SumResponse>
{
    public async Task<GeneratedRequests.Responses.SumResponse> ProcessRequestAsync(ClientRequest<GeneratedRequests.Requests.SumRequest, GeneratedRequests.Responses.SumResponse> request, CancellationToken cancellationToken)
    {
        GeneratedRequests.Responses.SumResponse response = new()
        {
            Result = request.Data.A + request.Data.B,
        };
        await request.SendResponseAsync(response, cancellationToken);
        return response;
    }
}