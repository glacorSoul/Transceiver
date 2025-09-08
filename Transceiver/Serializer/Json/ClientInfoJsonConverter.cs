// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Transceiver;

internal class ClientInfoJsonConverter : JsonConverter<IClientInfo>
{
    public override IClientInfo? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using JsonDocument jsonDoc = JsonDocument.ParseValue(ref reader);
        JsonElement root = jsonDoc.RootElement;

        return root.Deserialize<TransceiverHelloClientInfo>(options);
    }

    public override void Write(Utf8JsonWriter writer, IClientInfo value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        TransceiverHelloClientInfo clientInfo = (TransceiverHelloClientInfo)value;
        JsonSerializer.Serialize(writer, clientInfo, options);
    }
}