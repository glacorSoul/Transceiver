// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using Microsoft.Extensions.Options;
using Transceiver.Requests;

namespace Transceiver.Serializer;

internal class SerializerTransceiver : ISerializer
{
    private readonly IEnumerable<Type> TypesWithJsonDefault = [typeof(ServiceDiscoveryRequestModel), typeof(ServiceDiscoveryResponseModel)];
    private readonly ISerializer _serializer;
    private readonly SerializerJson _jsonSerializer;

    public SerializerTransceiver(ISerializer serializer, IOptions<TransceiverConfiguration> options)
    {
        _serializer = serializer;
        _jsonSerializer = new SerializerJson(options);
    }

    public object Deserialize(Type type, byte[] data)
    {
        if (TypesWithJsonDefault.Contains(type))
        {
            return _jsonSerializer.Deserialize(type, data);
        }
        return _serializer.Deserialize(type, data);
    }

    public T Deserialize<T>(byte[] data)
    {
        if (TypesWithJsonDefault.Contains(typeof(T)))
        {
            return _jsonSerializer.Deserialize<T>(data);
        }
        return _serializer.Deserialize<T>(data);
    }

    public Task<object> DeserializeAsync(Type type, byte[] data, CancellationToken cancellationToken)
    {
        if (TypesWithJsonDefault.Contains(type))
        {
            return _jsonSerializer.DeserializeAsync(type, data, cancellationToken);
        }
        return _serializer.DeserializeAsync(type, data, cancellationToken);
    }

    public Task<T> DeserializeAsync<T>(byte[] data, CancellationToken cancellationToken)
    {
        if (TypesWithJsonDefault.Contains(typeof(T)))
        {
            return _jsonSerializer.DeserializeAsync<T>(data, cancellationToken);
        }
        return _serializer.DeserializeAsync<T>(data, cancellationToken);
    }

    public byte[] Serialize<T>(T data)
    {
        if (TypesWithJsonDefault.Contains(typeof(T)))
        {
            return _jsonSerializer.Serialize(data);
        }
        return _serializer.Serialize(data);
    }

    public Task<byte[]> SerializeAsync(Type type, object data, CancellationToken cancellationToken)
    {
        if (TypesWithJsonDefault.Contains(type))
        {
            return _jsonSerializer.SerializeAsync(type, data, cancellationToken);
        }
        return _serializer.SerializeAsync(type, data, cancellationToken);
    }

    public Task<byte[]> SerializeAsync<T>(T data, CancellationToken cancellationToken)
    {
        if (TypesWithJsonDefault.Contains(typeof(T)))
        {
            return _jsonSerializer.SerializeAsync(data, cancellationToken);
        }
        return _serializer.SerializeAsync(data, cancellationToken);
    }

    public byte[] Seriazlize(Type type, object data)
    {
        if (TypesWithJsonDefault.Contains(type))
        {
            return _jsonSerializer.Seriazlize(type, data);
        }
        return _serializer.Seriazlize(type, data);
    }
}
