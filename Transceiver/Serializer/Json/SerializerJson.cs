// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Transceiver;

public sealed class SerializerJson : ISerializer
{
    private readonly JsonSerializerOptions _options;

    public SerializerJson(IOptions<TransceiverConfiguration> configurationOptions)
    {
        JsonSerializerOptions options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            AllowTrailingCommas = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        options.Converters.Add(new TransceiverHelloJsonConverter(configurationOptions));
        options.Converters.Add(new ClientInfoJsonConverter());
        options.Converters.Add(new IdentifiableConverter());
        _options = options;
    }

    public T Deserialize<T>(byte[] data)
    {
        return JsonSerializer.Deserialize<T>(data, _options)!;
    }

    public object Deserialize(Type type, byte[] data)
    {
        return JsonSerializer.Deserialize(data, type, _options)!;
    }

    public async Task<T> DeserializeAsync<T>(byte[] data, CancellationToken cancellationToken)
    {
        using MemoryStream output = new(data);
        T? result = await JsonSerializer.DeserializeAsync<T>(output, _options, cancellationToken);
        return result!;
    }

    public async Task<object> DeserializeAsync(Type type, byte[] data, CancellationToken cancellationToken)
    {
        using MemoryStream output = new(data);
        object? result = await JsonSerializer.DeserializeAsync(output, type, _options, cancellationToken);
        return result!;
    }

    public byte[] Serialize<T>(T data)
    {
        byte[] sent = JsonSerializer.SerializeToUtf8Bytes(data, _options);
        return sent;
    }

    public async Task<byte[]> SerializeAsync<T>(T data, CancellationToken cancellationToken)
    {
        using MemoryStream output = new();
        await JsonSerializer.SerializeAsync(output, data, _options, cancellationToken);
        byte[] bytes = output.ToArray();
        return bytes;
    }

    public async Task<byte[]> SerializeAsync(Type type, object data, CancellationToken cancellationToken)
    {
        using MemoryStream output = new();
        await JsonSerializer.SerializeAsync(output, data, type, _options, cancellationToken);
        byte[] bytes = output.ToArray();
        return bytes;
    }

    public byte[] Seriazlize(Type type, object data)
    {
        byte[] sent = JsonSerializer.SerializeToUtf8Bytes(data, type, _options);
        return sent;
    }
}