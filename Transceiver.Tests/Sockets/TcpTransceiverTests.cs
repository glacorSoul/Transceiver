// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using Microsoft.Extensions.DependencyInjection;

namespace Transceiver.Tests.Sockets;

[Trait("Category", "Transceiver")]
[Collection(nameof(TestCollectionSetup))]
public class TcpTransceiverTests : IClassFixture<TestFixture>
{
    private readonly TestFixture _fixture;
    private readonly ITransceiver<TcpSumRequest, TcpSumResponse> _transceiver;

    public TcpTransceiverTests(TestFixture fixture)
    {
        _fixture = fixture;
        _transceiver = _fixture.Provider.GetService<ITransceiver<TcpSumRequest, TcpSumResponse>>()!;
    }

    [Fact]
    public async Task SendAsync_ShouldNotError()
    {
        TcpSumRequest request = new(1, 2);
        ClientRequest<TcpSumRequest, TcpSumResponse> clientRequest = await _transceiver.SendToServerAsync(request, TestContext.Current.CancellationToken);
        Assert.Equivalent(clientRequest.Data, request);
    }

    [Fact]
    public async Task Transceive_ShouldSendAndReceive()
    {
        TcpSumRequest request = new(1, 2);
        TcpSumResponse sumResponse = await _transceiver.TransceiveOnceAsync(request, TestContext.Current.CancellationToken);
        Assert.Equivalent(request.A + request.B, sumResponse.Result);
    }
}