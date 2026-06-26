// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using MessagePack;
using MessagePack.Formatters;

namespace Transceiver.Serializer.SerializerMessagePack;

internal class IdentifiableMessagePackFormatter : IMessagePackFormatter<IIdentifiable?>
{
    public void Serialize(ref MessagePackWriter writer, IIdentifiable? value, MessagePackSerializerOptions options)
    {
        Type[] genericArguments = value!.GetType().IsGenericType ? value.GetType().GetGenericArguments() : [];

        if (genericArguments.Length == 1)
        {
            Type serverResponseType = typeof(ServerResponseMessagePack<>);
            Type currentServerResponseType = serverResponseType.MakeGenericType(genericArguments[genericArguments.Length - 1]);
            MessagePackSerializer.Serialize(currentServerResponseType, ref writer, value, options);
        }
        else if (genericArguments.Length == 2)
        {
            Type clientRequestType = typeof(ClientRequestMessagePack<,>);
            Type currentRequestType = clientRequestType.MakeGenericType(genericArguments);
            MessagePackSerializer.Serialize(currentRequestType, ref writer, value, options);
        }
        else
        {
            throw new NotSupportedException("Only ClientRequestMessagePack<TRequest, TResponse> and ServerResponseMessagePack<TResponse> are supported.");
        }
    }

    public IIdentifiable Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        return (IIdentifiable)MessagePackSerializer.Deserialize<object>(ref reader, options);
    }
}
