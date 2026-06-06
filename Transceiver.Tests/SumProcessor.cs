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
    public async Task<TcpSumResponse> ProcessRequestAsync(ClientRequest<TcpSumRequest, TcpSumResponse> request, CancellationToken cancellationToken)
    {
        TcpSumResponse response = new(request.Data.A + request.Data.B);
        await request.SendResponseAsync(response, cancellationToken);
        return response;
    }

    public async Task<ChannelsSumResponse> ProcessRequestAsync(ClientRequest<ChannelsSumRequest, ChannelsSumResponse> request, CancellationToken cancellationToken)
    {
        ChannelsSumResponse response = new(request.Data.A + request.Data.B);
        await request.SendResponseAsync(response, cancellationToken);
        return response;
    }

    public async Task<DomainSocketsSumResponse> ProcessRequestAsync(ClientRequest<DomainSocketsSumRequest, DomainSocketsSumResponse> request, CancellationToken cancellationToken)
    {
        DomainSocketsSumResponse response = new(request.Data.A + request.Data.B);
        await request.SendResponseAsync(response, cancellationToken);
        return response;
    }

    public async Task<UdpSumResponse> ProcessRequestAsync(ClientRequest<UdpSumRequest, UdpSumResponse> request, CancellationToken cancellationToken)
    {
        UdpSumResponse response = new(request.Data.A + request.Data.B);
        await request.SendResponseAsync(response, cancellationToken);
        return response;
    }
}