// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

namespace Transceiver.Demo;

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

public sealed class SumProcessor : IProcessor<SumRequest, SumResponse>
{
    public Task<SumResponse> ProcessRequest(SumRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new SumResponse(request.A + request.B));
    }
}