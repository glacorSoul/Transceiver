// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

namespace Transceiver.Tests;

public class SumProcessor :
    IProcessor<UdpSumRequest, UdpSumResponse>,
    IProcessor<TcpSumRequest, TcpSumResponse>,
    IProcessor<DomainSocketsSumRequest, DomainSocketsSumResponse>,
    IProcessor<ChannelsSumRequest, ChannelsSumResponse>
{
    public Task<TcpSumResponse> ProcessRequest(TcpSumRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new TcpSumResponse(request.A + request.B));
    }

    public Task<ChannelsSumResponse> ProcessRequest(ChannelsSumRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new ChannelsSumResponse(request.A + request.B));
    }

    public Task<DomainSocketsSumResponse> ProcessRequest(DomainSocketsSumRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new DomainSocketsSumResponse(request.A + request.B));
    }

    public Task<UdpSumResponse> ProcessRequest(UdpSumRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new UdpSumResponse(request.A + request.B));
    }
}