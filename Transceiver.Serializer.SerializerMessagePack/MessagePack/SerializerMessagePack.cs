// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;

namespace Transceiver.Serializer.SerializerMessagePack;

public class SerializerMessagePack : ISerializer
{
    private readonly MessagePackSerializerOptions _messagePackOptions;
    public SerializerMessagePack() : this(DefaultOptions())
    {
    }

    public SerializerMessagePack(MessagePackSerializerOptions messagePackOptions)
    {
        _messagePackOptions = messagePackOptions;
    }

    private static MessagePackSerializerOptions DefaultOptions()
    {
        IFormatterResolver resolver = CompositeResolver.Create(
            new IMessagePackFormatter[]
            {
                new IdentifiableMessagePackFormatter()
            },
            new IFormatterResolver[]
            {
                StandardResolver.Instance,
                ServerResponseResolver.Instance
        });
        MessagePackSerializerOptions options = MessagePackSerializerOptions.Standard.WithResolver(resolver);
        return options;
    }

    public object Deserialize(Type type, byte[] data)
    {
        return MessagePackSerializer.Deserialize(type, data, _messagePackOptions)!;
    }

    public T Deserialize<T>(byte[] data)
    {
        return MessagePackSerializer.Deserialize<T>(data, _messagePackOptions);
    }

    public async Task<object> DeserializeAsync(Type type, byte[] data, CancellationToken cancellationToken)
    {
        using Stream stream = new MemoryStream(data);
        object result = (await MessagePackSerializer.DeserializeAsync(type, stream, _messagePackOptions, cancellationToken))!;
        return result;
    }

    public async Task<T> DeserializeAsync<T>(byte[] data, CancellationToken cancellationToken)
    {
        using Stream stream = new MemoryStream(data);
        T result = (await MessagePackSerializer.DeserializeAsync<T>(stream, _messagePackOptions, cancellationToken))!;
        return result;
    }

    public byte[] Serialize<T>(T data)
    {
        return MessagePackSerializer.Serialize<T>(data, _messagePackOptions);
    }

    public async Task<byte[]> SerializeAsync(Type type, object data, CancellationToken cancellationToken)
    {
        using MemoryStream stream = new MemoryStream();
        await MessagePackSerializer.SerializeAsync(stream, data, _messagePackOptions, cancellationToken);
        return stream.ToArray();
    }

    public async Task<byte[]> SerializeAsync<T>(T data, CancellationToken cancellationToken)
    {
        using MemoryStream stream = new MemoryStream();
        await MessagePackSerializer.SerializeAsync<T>(stream, data, _messagePackOptions, cancellationToken);
        return stream.ToArray();
    }

    public byte[] Seriazlize(Type type, object data)
    {
        return MessagePackSerializer.Serialize(type, data, _messagePackOptions);
    }
}
