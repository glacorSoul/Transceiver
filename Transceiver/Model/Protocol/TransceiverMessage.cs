// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using System.Text;

namespace Transceiver;

public sealed class TransceiverMessage : IIdentifiable
{
    public TransceiverMessage(IIdentifiable data, ISerializer serializer)
    {
        Data = serializer.Seriazlize(data.GetType(), data);
        Header = TransceiverHeader.CreateHeader(data.GetType(), Data.Length, data.Id);
    }

    public TransceiverMessage(TransceiverHeader header, ReadOnlyMemory<byte> data)
    {
        Header = header;
        Data = data;
    }

    public TransceiverMessage(byte[] data)
    {
        if (data.Length < TransceiverHeader.Size)
        {
            throw new ArgumentException($"Data length {data.Length} is less than header size {TransceiverHeader.Size}");
        }
        Header = new(data);
        if (Header.MessageSize > 0)
        {
            Data = new ReadOnlyMemory<byte>(data, TransceiverHeader.Size, data.Length - TransceiverHeader.Size);
        }
        else
        {
            Data = ReadOnlyMemory<byte>.Empty;
        }
    }

    public TransceiverMessage(string csv)
    {
        Header = new(csv);
        string text = csv.Substring(csv.IndexOf(',', TransceiverHeader.MinHeaderSizeString) + 1);
        Data = Encoding.UTF8.GetBytes(text);
    }

    public ReadOnlyMemory<byte> Data { get; }

    public TransceiverHeader Header { get; }

    public Guid Id => Header.Id;

    public byte[] ToBytes()
    {
        byte[] messageBytes = new byte[TransceiverHeader.Size + Data.Length];
        return ToBytes(messageBytes);
    }

    public byte[] ToBytes(byte[] buffer)
    {
        if (buffer.Length != TransceiverHeader.Size + Data.Length)
        {
            throw new ArgumentException($"Buffer length {buffer.Length} does not match required size {TransceiverHeader.Size + Data.Length}", nameof(buffer));
        }
        Header.WriteTo(buffer.AsSpan(0, TransceiverHeader.Size));
        if (Data.Length > 0)
        {
            Data.Span.CopyTo(buffer.AsSpan(TransceiverHeader.Size));
        }
        return buffer;
    }

    public string ToCsv()
    {
        string headerText = Header.ToCsv();
        string dataText = Encoding.UTF8.GetString(Data.ToArray());
        return $"{headerText},{dataText}";
    }
}