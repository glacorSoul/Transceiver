// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using Microsoft.Extensions.Options;
using Moq;
using System.Text;

namespace Transceiver.Tests.Serializer;

[Trait("Category", "Serializer")]
public class TransceiverHelloJsonConverterAsyncTest
{
    private readonly SerializerJson _serializer;

    public TransceiverHelloJsonConverterAsyncTest()
    {
        Mock<IOptions<TransceiverConfiguration>> _configMock = new();
        _ = _configMock.Setup(m => m.Value).Returns(new TransceiverConfiguration()
        {
            OptimizeHelloSerialization = true
        });
        _serializer = new SerializerJson(_configMock.Object);
    }

    [Fact]
    public async Task DeserializeClientInfo_MultipleDeserialize_ShouldDeserializeAllData()
    {
        TransceiverHelloIdentifier identifier = new();

        await DeserializeAndAssertClientInfo(identifier);
        await DeserializeAndAssertClientInfo(identifier);
    }

    [Fact]
    public async Task DeserializeClientInfo_SingleDeserialize_ShouldDeserializeAllData()
    {
        TransceiverHelloClientInfo clientInfo = new(new(), new(new("a", ProtocolTypeEnum.Tcp)));
        byte[] bytes = await _serializer.SerializeAsync(clientInfo, TestContext.Current.CancellationToken);
        TransceiverHelloClientInfo clientInfo2 = await _serializer.DeserializeAsync<TransceiverHelloClientInfo>(bytes, TestContext.Current.CancellationToken);
        Assert.Equivalent(clientInfo, clientInfo2);
    }

    [Fact]
    public async Task SerializeClientInfo_MultipleSerialize_ShouldSerializeOnlyId()
    {
        TransceiverHelloIdentifier identifier = new();

        await SerializeAndAssertClientInfo(identifier, true);
        await SerializeAndAssertClientInfo(identifier, false);
    }

    [Fact]
    public async Task SerializeClientInfo_SingleSerialize_ShouldSerializeAllData()
    {
        TransceiverHelloClientInfo clientInfo = new(new(), new(new("a", ProtocolTypeEnum.Tcp)));
        byte[] bytes = await _serializer.SerializeAsync(clientInfo, TestContext.Current.CancellationToken);
        string json = Encoding.UTF8.GetString(bytes);
        string expectedJson = $"{{\"Id\":\"{clientInfo.Id.Id}\",\"Data\":{{\"clientIdentifier\":{{\"endPoint\":\"a\",\"protocolType\":{{\"name\":\"{clientInfo.Data.ClientIdentifier.ProtocolType.Name}\",\"value\":{clientInfo.Data.ClientIdentifier.ProtocolType.Value}}}}}}}}}";
        Assert.Equal(expectedJson, json);
    }

    private async Task DeserializeAndAssertClientInfo(TransceiverHelloIdentifier identifier)
    {
        TransceiverHelloClientInfo clientInfo = new(identifier, new(new("a", ProtocolTypeEnum.Tcp)));
        byte[] bytes = await _serializer.SerializeAsync(clientInfo, TestContext.Current.CancellationToken);
        TransceiverHelloClientInfo clientInfo2 = await _serializer.DeserializeAsync<TransceiverHelloClientInfo>(bytes, TestContext.Current.CancellationToken);
        Assert.Equivalent(clientInfo, clientInfo2);
    }

    private async Task SerializeAndAssertClientInfo(TransceiverHelloIdentifier identifier, bool includeData)
    {
        TransceiverHelloClientInfo clientInfo = new(identifier, new(new("a", ProtocolTypeEnum.Tcp)));
        byte[] bytes = await _serializer.SerializeAsync(clientInfo, TestContext.Current.CancellationToken);
        string json = Encoding.UTF8.GetString(bytes);

        string expectedJson = includeData
            ? $"{{\"Id\":\"{clientInfo.Id.Id}\",\"Data\":{{\"clientIdentifier\":{{\"endPoint\":\"a\",\"protocolType\":{{\"name\":\"{clientInfo.Data.ClientIdentifier.ProtocolType.Name}\",\"value\":{clientInfo.Data.ClientIdentifier.ProtocolType.Value}}}}}}}}}"
            : $"{{\"Id\":\"{clientInfo.Id.Id}\"}}";

        Assert.Equal(expectedJson, json);
    }
}