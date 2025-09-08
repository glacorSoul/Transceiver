// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using Microsoft.Extensions.DependencyInjection;

namespace Transceiver;

public class TransceiverHeader
{
    public const int Size = sizeof(int) + sizeof(ushort) + SizeOfGuid;
    private const int SizeOfGuid = 16;
    private static readonly TypeIdAssigner _typeIdMap = BootStrap.ServiceProvider.GetRequiredService<TypeIdAssigner>();

    public TransceiverHeader(byte[] headerBuffer)
    {
        Span<byte> span = headerBuffer.AsSpan();
        FromSpan(span);
    }

    public TransceiverHeader(ReadOnlySpan<byte> span)
    {
        FromSpan(span);
    }

    public TransceiverHeader(string csv)
    {
        string[] parts = csv.Split(',');
        MessageSize = int.Parse(parts[0]);
        TypeId = ushort.Parse(parts[1]);
        Id = Guid.Parse(parts[2]);
    }

    private TransceiverHeader()
    {
    }

    public Guid Id { get; set; }

    public int MessageSize { get; private set; }

    public Type Type
    {
        get
        {
            return _typeIdMap.GetType(TypeId);
        }
    }

    public ushort TypeId { get; private set; }

    public static TransceiverHeader CreateHeader(Type type, int dataLength, Guid requestId)
    {
        ushort typeId = _typeIdMap.GetTypeId(type);
        TransceiverHeader header = new()
        {
            MessageSize = dataLength,
            TypeId = typeId,
            Id = requestId
        };
        return header;
    }

    public byte[] ToArray()
    {
        byte[] buffer = new byte[Size];
        WriteTo(buffer);
        return buffer;
    }

    public string ToCsv()
    {
        return $"{MessageSize},{TypeId},{Id}";
    }

    public override string ToString()
    {
        return $"{MessageSize} ({TypeId}: {Type.ToShortPrettyString()}) {Id}";
    }

    public void WriteTo(Span<byte> span)
    {
        Span<byte> lengthSpan = span.Slice(0, sizeof(int));
        Span<byte> typeIdSpan = span.Slice(sizeof(int), sizeof(ushort));
        Span<byte> guidSpan = span.Slice(sizeof(int) + sizeof(ushort), SizeOfGuid);

        lengthSpan[0] = (byte)(MessageSize & 0xFF);
        lengthSpan[1] = (byte)((MessageSize >> 8) & 0xFF);
        lengthSpan[2] = (byte)((MessageSize >> 16) & 0xFF);
        lengthSpan[3] = (byte)((MessageSize >> 24) & 0xFF);

        typeIdSpan[0] = (byte)(TypeId & 0xFF);
        typeIdSpan[1] = (byte)((TypeId >> 8) & 0xFF);

        byte[] guidArray = Id.ToByteArray();
        guidArray.CopyTo(guidSpan);
    }

    public void WriteTo(byte[] buffer)
    {
        Span<byte> span = buffer.AsSpan();
        WriteTo(span);
    }

    private void FromSpan(ReadOnlySpan<byte> span)
    {
        ReadOnlySpan<byte> lengthSpan = span.Slice(0, sizeof(int));
        ReadOnlySpan<byte> typeIdSpan = span.Slice(sizeof(int), sizeof(ushort));
        ReadOnlySpan<byte> guidSpan = span.Slice(sizeof(int) + sizeof(ushort), SizeOfGuid);

        MessageSize = lengthSpan[0] | (lengthSpan[1] << 8) | (lengthSpan[2] << 16) | (lengthSpan[3] << 24);
        TypeId = (ushort)(typeIdSpan[0] | (typeIdSpan[1] << 8));
        Id = new Guid(guidSpan.ToArray());
    }
}