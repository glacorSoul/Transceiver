// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using MessagePack;
using MessagePack.Formatters;

namespace Transceiver.Serializer.SerializerMessagePack;

public sealed class ServerResponseResolver : IFormatterResolver
{
    public static readonly IFormatterResolver Instance = new ServerResponseResolver();

    private ServerResponseResolver()
    {
    }

    public IMessagePackFormatter<T>? GetFormatter<T>()
    {
        return FormatterCache<T>.Formatter;
    }

    private static class FormatterCache<T>
    {
        public static readonly IMessagePackFormatter<T>? Formatter = CreateFormatter();

        static IMessagePackFormatter<T>? CreateFormatter()
        {
            Type type = typeof(T);

            if (!type.IsGenericType)
                return null;

            Type genericTypeDef = type.GetGenericTypeDefinition();

            if (genericTypeDef == typeof(ServerResponseMessagePack<>))
            {
                Type[] args = type.GetGenericArguments();
                Type formatterType = typeof(ServerResponseMessagePackFormatter<>)
                    .MakeGenericType(args[0]);

                return (IMessagePackFormatter<T>)Activator.CreateInstance(formatterType)!;
            }
            if (genericTypeDef == typeof(ClientRequest<,>))
            {
                Type[] args = type.GetGenericArguments();
                Type formatterType = typeof(ClientRequest<,>)
                    .MakeGenericType(args[0]);

                return (IMessagePackFormatter<T>)Activator.CreateInstance(formatterType)!;
            }

            return null;
        }
    }
}
