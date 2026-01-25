// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

namespace Transceiver.Tests;

public class SumProcessor :
    IProcessor<TcpSumRequest, TcpSumResponse>,
    IProcessor<DomainSocketsSumRequest, DomainSocketsSumResponse>,
    IProcessor<ChannelsSumRequest, ChannelsSumResponse>,
    IProcessor<UdpSumRequest, UdpSumResponse>
{
    public Task<TcpSumResponse> ProcessRequestAsync(TcpSumRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new TcpSumResponse(request.A + request.B));
    }

    public Task<ChannelsSumResponse> ProcessRequestAsync(ChannelsSumRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new ChannelsSumResponse(request.A + request.B));
    }

    public Task<DomainSocketsSumResponse> ProcessRequestAsync(DomainSocketsSumRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new DomainSocketsSumResponse(request.A + request.B));
    }

    public Task<UdpSumResponse> ProcessRequestAsync(UdpSumRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new UdpSumResponse(request.A + request.B));
    }
}