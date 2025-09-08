// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Transceiver;

internal class TransceiverHelloJsonConverter : JsonConverter<TransceiverHelloClientInfo>
{
    private static readonly Dictionary<TransceiverHelloIdentifier, TransceiverHelloClientInfo> _dataMap = [];
    private readonly IOptions<TransceiverConfiguration> _config;

    public TransceiverHelloJsonConverter(IOptions<TransceiverConfiguration> config)
    {
        _config = config;
    }

    public override TransceiverHelloClientInfo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        Guid id = Guid.Empty;
        TransceiverHelloData? data = null;

        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected StartObject token");
        }

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                break;
            }

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                string propertyName = reader.GetString() ?? string.Empty;
                _ = reader.Read();

                switch (propertyName)
                {
                    case "Id":
                    {
                        id = reader.GetGuid();
                        break;
                    }
                    case "Data":
                    {
                        data = JsonSerializer.Deserialize<TransceiverHelloData>(ref reader, options);
                        break;
                    }
                    default:
                    {
                        reader.Skip();
                        break;
                    }
                }
            }
        }

        if (data != null)
        {
            return new TransceiverHelloClientInfo(new(id), data);
        }

        if (id == Guid.Empty)
        {
            throw new JsonException("Id property is required");
        }

        lock (_dataMap)
        {
            TransceiverHelloIdentifier key = new(id);
            if (!_dataMap.TryGetValue(key, out TransceiverHelloClientInfo? storedData))
            {
                throw new JsonException("Data property is required or should be cached");
            }

            return storedData!;
        }
    }

    public override void Write(Utf8JsonWriter writer, TransceiverHelloClientInfo value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WritePropertyName("Id");
        writer.WriteStringValue(value.Id.Id);

        bool serializeData = !_config.Value.OptimizeHelloSerialization;
        lock (_dataMap)
        {
            if (!_dataMap.ContainsKey(value.Id))
            {
                _dataMap[value.Id] = value;
                serializeData = true;
            }
        }
        if (serializeData)
        {
            writer.WritePropertyName("Data");
            JsonSerializer.Serialize(writer, value.Data, options);
        }

        writer.WriteEndObject();
    }
}