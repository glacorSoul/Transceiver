// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using System.Buffers;
using System.Text;

namespace Transceiver;

internal class PartitionedMessage
{
    private byte[] _buffer = new byte[TransceiverHeader.Size];
    private int _count;
    private byte[]? _rentedPayload;
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

        EnsureCapacity(_count + nRead);
        Buffer.BlockCopy(buffer, 0, _buffer, _count, nRead);
        _count += nRead;
        BytesRead += nRead;

        if (Header == null && BytesRead >= TransceiverHeader.Size)
        {
            Header = new TransceiverHeader(new ReadOnlySpan<byte>(_buffer, 0, TransceiverHeader.Size));
        }

        if (Header != null && BytesRead >= TransceiverHeader.Size + Header.MessageSize)
        {
            _rentedPayload = ArrayPool<byte>.Shared.Rent(Header.MessageSize);
            Buffer.BlockCopy(_buffer, TransceiverHeader.Size, _rentedPayload, 0, Header.MessageSize);
            Message = new(Header, new ReadOnlyMemory<byte>(_rentedPayload, 0, Header.MessageSize));
            IsCompleted = true;

            int consumed = TransceiverHeader.Size + Header.MessageSize;
            int remainingCount = _count - consumed;
            if (remainingCount > 0)
            {
                byte[] remaining = ArrayPool<byte>.Shared.Rent(remainingCount);
                Buffer.BlockCopy(_buffer, consumed, remaining, 0, remainingCount);
                RemainingBytes = new ArraySegment<byte>(remaining, 0, remainingCount);
            }
        }
    }

    public void ReturnPayloadBuffer()
    {
        if (_rentedPayload != null)
        {
            ArrayPool<byte>.Shared.Return(_rentedPayload);
            _rentedPayload = null;
        }
    }

    private void EnsureCapacity(int requiredCapacity)
    {
        if (requiredCapacity <= _buffer.Length)
        {
            return;
        }

        int newCapacity = _buffer.Length * 2;
        if (newCapacity < requiredCapacity)
        {
            newCapacity = requiredCapacity;
        }

        Array.Resize(ref _buffer, newCapacity);
    }

    public void Reset()
    {
        BytesRead = 0;
        Header = null;
        IsCompleted = false;
        Message = null;
        RemainingBytes = default;
        _count = 0;
    }

    public override string ToString()
    {
        if (Header == null || Message == null)
        {
            return $"[Incomplete message: {BytesRead} bytes read]";
        }
        return $"{Header}\n{Encoding.UTF8.GetString(Message.Data.ToArray())}";
    }
}