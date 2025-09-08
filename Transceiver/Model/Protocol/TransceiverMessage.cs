// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using System.Text;

namespace Transceiver;

public sealed class TransceiverMessage : IIdentifiable
{
    public TransceiverMessage(IIdentifiable data, ISerializer serializer)
    {
        Data = serializer.Serialize(data);
        Header = TransceiverHeader.CreateHeader(data.GetType(), Data.Length, data.Id);
    }

    public TransceiverMessage(TransceiverHeader header, byte[] data)
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
            Data = [.. data.Skip(TransceiverHeader.Size)];
        }
        else
        {
            Data = [];
        }
    }

    public TransceiverMessage(string csv)
    {
        Header = new(csv);
        string text = csv.Substring(csv.IndexOf(',', 32 + 4) + 1);
        Data = Encoding.UTF8.GetBytes(text);
    }

    public byte[] Data { get; }

    public TransceiverHeader Header { get; }

    public Guid Id => Header.Id;

    public byte[] ToBytes()
    {
        byte[] headerBytes = Header.ToArray();
        byte[] messageBytes = new byte[headerBytes.Length + Data.Length];
        Buffer.BlockCopy(headerBytes, 0, messageBytes, 0, headerBytes.Length);
        if (Data.Length > 0)
        {
            Buffer.BlockCopy(Data, 0, messageBytes, headerBytes.Length, Data.Length);
        }
        return messageBytes;
    }

    public string ToCsv()
    {
        string headerText = Header.ToCsv();
        string dataText = Encoding.UTF8.GetString(Data);
        return $"{headerText},{dataText}";
    }
}