// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using MessagePack;

namespace Transceiver.Demo;

public sealed class SumProcessor : IProcessor<SumRequest, SumResponse>
{
    public async Task<SumResponse> ProcessRequestAsync(IClientRequest<SumRequest, SumResponse> request, CancellationToken cancellationToken)
    {
        SumResponse response = new(request.Data.A + request.Data.B);
        await request.SendResponseAsync(response, cancellationToken);
        return response;
    }
}

[MessagePackObject]
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

    [Key(0)]
    public int A { get; set; }
    [Key(1)]
    public int B { get; set; }
}

[MessagePackObject]
public sealed class SumResponse
{
    public SumResponse()
    {
    }

    public SumResponse(int result)
    {
        Result = result;
    }

    [Key(0)]
    public int Result { get; set; }
}