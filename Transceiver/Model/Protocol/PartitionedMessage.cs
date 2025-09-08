// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using System.Text;

namespace Transceiver;

internal class PartitionedMessage
{
    private readonly List<ArraySegment<byte>> _buffers = [];
    public int BytesRead { get; set; }
    public TransceiverHeader? Header { get; set; }
    public bool IsCompleted { get; set; }
    public TransceiverMessage? Message { get; set; }
    public ArraySegment<byte> RemainingBytes { get; set; }

    public void ParseBuffer(byte[] buffer, int nRead)
    {
        if (IsCompleted)
        {
            return;
        }
        int offset = 0;
        BytesRead += nRead;

        if (Header == null && BytesRead >= TransceiverHeader.Size)
        {
            Header = new TransceiverHeader(buffer);
            offset += TransceiverHeader.Size;
        }
        _buffers.Add(new ArraySegment<byte>(buffer, offset, nRead - offset));

        if (Header != null && BytesRead >= Header.MessageSize + offset)
        {
            byte[] rawValue = GetBuffersAsSingleBuffer();
            Message = new(Header, rawValue);
            IsCompleted = true;
            offset += Header.MessageSize;
        }

        if (nRead > offset)
        {
            RemainingBytes = new ArraySegment<byte>(buffer, offset, nRead - offset);
        }
    }

    public override string ToString()
    {
        return $"{Header!}\n{Encoding.UTF8.GetString(GetBuffersAsSingleBuffer())}";
    }

    private byte[] GetBuffersAsSingleBuffer()
    {
        if (Header == null)
        {
            throw new InvalidOperationException("Header must be set before retrieving the buffer.");
        }
        byte[] buffer = new byte[Header.MessageSize];

        int offset = 0;
        foreach (ArraySegment<byte> oldBuffer in _buffers)
        {
            Array.Copy(oldBuffer.Array!, oldBuffer.Offset, buffer, offset, Header.MessageSize - offset);
            offset += oldBuffer.Count;
        }

        return buffer;
    }
}