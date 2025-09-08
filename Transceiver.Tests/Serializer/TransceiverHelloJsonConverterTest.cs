// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using Microsoft.Extensions.Options;
using Moq;
using System.Text;

namespace Transceiver.Tests.Serializer;

[Trait("Category", "Serializer")]
public class TransceiverHelloJsonConverterTest
{
    private readonly SerializerJson _serializer;

    public TransceiverHelloJsonConverterTest()
    {
        Mock<IOptions<TransceiverConfiguration>> _configMock = new();
        _ = _configMock.Setup(m => m.Value).Returns(new TransceiverConfiguration()
        {
            OptimizeHelloSerialization = true
        });
        _serializer = new SerializerJson(_configMock.Object);
    }

    [Fact]
    public void DeserializeClientInfo_MultipleDeserialize_ShouldDeserializeAllData()
    {
        TransceiverHelloIdentifier identifier = new();

        DeserializeAndAssertClientInfo(identifier);
        DeserializeAndAssertClientInfo(identifier);
    }

    [Fact]
    public void DeserializeClientInfo_SingleDeserialize_ShouldDeserializeAllData()
    {
        TransceiverHelloClientInfo clientInfo = new(new(), new(new("a", ProtocolTypeEnum.Tcp)));
        byte[] bytes = _serializer.Serialize(clientInfo);
        TransceiverHelloClientInfo clientInfo2 = _serializer.Deserialize<TransceiverHelloClientInfo>(bytes);
        Assert.Equivalent(clientInfo, clientInfo2);
    }

    [Fact]
    public void SerializeClientInfo_MultipleSerialize_ShouldSerializeOnlyId()
    {
        TransceiverHelloIdentifier identifier = new();

        SerializeAndAssertClientInfo(identifier, true);
        SerializeAndAssertClientInfo(identifier, false);
    }

    [Fact]
    public void SerializeClientInfo_SingleSerialize_ShouldSerializeAllData()
    {
        TransceiverHelloClientInfo clientInfo = new(new(), new(new("a", ProtocolTypeEnum.Tcp)));
        byte[] bytes = _serializer.Serialize(clientInfo);
        string json = Encoding.UTF8.GetString(bytes);
        string expectedJson = $"{{\"Id\":\"{clientInfo.Id.Id}\",\"Data\":{{\"clientIdentifier\":{{\"endPoint\":\"a\",\"protocolType\":{{\"name\":\"{clientInfo.Data.ClientIdentifier.ProtocolType.Name}\",\"value\":{clientInfo.Data.ClientIdentifier.ProtocolType.Value}}}}}}}}}";
        Assert.Equal(expectedJson, json);
    }

    private void DeserializeAndAssertClientInfo(TransceiverHelloIdentifier identifier)
    {
        TransceiverHelloClientInfo clientInfo = new(identifier, new(new("a", ProtocolTypeEnum.Tcp)));
        byte[] bytes = _serializer.Serialize(clientInfo);
        TransceiverHelloClientInfo clientInfo2 = _serializer.Deserialize<TransceiverHelloClientInfo>(bytes);
        Assert.Equivalent(clientInfo, clientInfo2);
    }

    private void SerializeAndAssertClientInfo(TransceiverHelloIdentifier identifier, bool includeData)
    {
        TransceiverHelloClientInfo clientInfo = new(identifier, new(new("a", ProtocolTypeEnum.Tcp)));
        byte[] bytes = _serializer.Serialize(clientInfo);
        string json = Encoding.UTF8.GetString(bytes);

        string expectedJson = includeData
            ? $"{{\"Id\":\"{clientInfo.Id.Id}\",\"Data\":{{\"clientIdentifier\":{{\"endPoint\":\"a\",\"protocolType\":{{\"name\":\"{clientInfo.Data.ClientIdentifier.ProtocolType.Name}\",\"value\":{clientInfo.Data.ClientIdentifier.ProtocolType.Value}}}}}}}}}"
            : $"{{\"Id\":\"{clientInfo.Id.Id}\"}}";

        Assert.Equal(expectedJson, json);
    }
}