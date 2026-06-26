// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using MessagePack;
using MessagePack.Formatters;

namespace Transceiver.Serializer.SerializerMessagePack;

public sealed class ServerResponseMessagePackFormatter<TResponse>
    : IMessagePackFormatter<ServerResponseMessagePack<TResponse>?>
{
    public void Serialize(
        ref MessagePackWriter writer,
        ServerResponseMessagePack<TResponse>? value,
        MessagePackSerializerOptions options)
    {
        writer.WriteArrayHeader(3);

        writer.Write(value!.TimeStamp.DateTime);

        MessagePackSerializer.Serialize(ref writer, value.Data, options);

        writer.Write(value.Id.ToByteArray());
    }

    public ServerResponseMessagePack<TResponse> Deserialize(
        ref MessagePackReader reader,
        MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil())
        {
            return default!;
        }

        options.Security.DepthStep(ref reader);
        try
        {
            int count = reader.ReadArrayHeader();

            DateTimeOffset timeStamp = default;
            TResponse data = default!;
            Guid id = Guid.Empty;

            for (int i = 0; i < count; i++)
            {
                switch (i)
                {
                    case 0:
                    timeStamp = reader.ReadDateTime();
                    break;

                    case 1:
                    data = MessagePackSerializer.Deserialize<TResponse>(ref reader, options);
                    break;

                    case 2:
                    id = new Guid(reader.ReadBytes()!.Value.First.ToArray());
                    break;

                    default:
                    reader.Skip();
                    break;
                }
            }

            return new ServerResponseMessagePack<TResponse>(data, id, timeStamp);
        }
        finally
        {
            reader.Depth--;
        }
    }
}
