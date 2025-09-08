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

    public TcpTransceiverTests(TestFixture fixture)
    {
        _fixture = fixture;
        Transceiver = _fixture.Provider.GetService<ITransceiver<TcpSumRequest, TcpSumResponse>>()!;
        _ = Transceiver.StartProcessingRequestsAsync(new SumProcessor(), TestContext.Current.CancellationToken).ConfigureAwait(false);
    }

    public ITransceiver<TcpSumRequest, TcpSumResponse> Transceiver { get; }

    [Fact]
    public async Task SendAsync_ShouldNotError()
    {
        TcpSumRequest request = new(1, 2);
        ClientRequest<TcpSumRequest, TcpSumResponse> clientRequest = await Transceiver.SendToServerAsync(request, TestContext.Current.CancellationToken);
        Assert.Equivalent(clientRequest.Data, request);
    }

    [Fact]
    public async Task Transceive_ShouldSendAndReceive()
    {
        TcpSumRequest request = new(1, 2);
        TcpSumResponse sumResponse = await Transceiver.TransceiveOnceAsync(request, TestContext.Current.CancellationToken);
        Assert.Equivalent(request.A + request.B, sumResponse.Result);
    }
}