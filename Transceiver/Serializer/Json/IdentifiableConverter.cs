// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Transceiver;

internal class IdentifiableConverter : JsonConverter<IIdentifiable>
{
    public override IIdentifiable? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using JsonDocument jsonDoc = JsonDocument.ParseValue(ref reader);
        JsonElement root = jsonDoc.RootElement;

        return (IIdentifiable)root.Deserialize(typeToConvert, options)!;
    }

    public override void Write(Utf8JsonWriter writer, IIdentifiable value, JsonSerializerOptions options)
    {
        Type[] genericArguments = value.GetType().IsGenericType ? value.GetType().GetGenericArguments() : [];
        Type clientRequestType = typeof(ClientRequest<,>);
        Type serverResponseType = typeof(ServerResponse<,>);

        if (genericArguments.Length != 2)
        {
            throw new NotSupportedException("Only ClientRequest<TRequest, TResponse> and ServerResponse<TRequest, TResponse> are supported.");
        }
        Type currentRequestType = clientRequestType.MakeGenericType(genericArguments);
        Type currentServerResponseType = serverResponseType.MakeGenericType(genericArguments);
        if (value.GetType() == currentRequestType)
        {
            JsonSerializer.Serialize(writer, value, currentRequestType, options);
        }
        else if (value.GetType() == currentServerResponseType)
        {
            JsonSerializer.Serialize(writer, value, currentServerResponseType, options);
        }
        else
        {
            throw new NotSupportedException("Only ClientRequest<TRequest, TResponse> and ServerResponse<TRequest, TResponse> are supported.");
        }
    }
}