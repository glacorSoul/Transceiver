// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using System.Text;

namespace Transceiver;

internal class PartitionedMessage
{
    private readonly List<byte> _buffer = [];
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

        _buffer.AddRange(new ArraySegment<byte>(buffer, 0, nRead));
        BytesRead += nRead;

        if (Header == null && BytesRead >= TransceiverHeader.Size)
        {
            Header = new TransceiverHeader(_buffer.GetRange(0, TransceiverHeader.Size).ToArray());
        }

        if (Header != null && BytesRead >= TransceiverHeader.Size + Header.MessageSize)
        {
            byte[] rawValue = _buffer.GetRange(TransceiverHeader.Size, Header.MessageSize).ToArray();
            Message = new(Header, rawValue);
            IsCompleted = true;

            int consumed = TransceiverHeader.Size + Header.MessageSize;
            int remainingCount = _buffer.Count - consumed;
            if (remainingCount > 0)
            {
                RemainingBytes = new ArraySegment<byte>([.. _buffer.GetRange(consumed, remainingCount)]);
            }
        }
    }

    public override string ToString()
    {
        if (Header == null || Message == null)
        {
            return $"[Incomplete message: {BytesRead} bytes read]";
        }
        return $"{Header}\n{Encoding.UTF8.GetString(Message.Data)}";
    }
}