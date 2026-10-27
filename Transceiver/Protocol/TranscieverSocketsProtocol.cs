// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace Transceiver;

public sealed class TransceiverSocketProtocol : ReceiveMessagesProtocol<Socket>
{
    private readonly Lazy<Task<Socket>> _setupWrite;
    private readonly ISocketFactory _socketFactory;
    private readonly ConcurrentBag<SocketAsyncEventArgs> _socketArgsPool = [];
    private readonly ConcurrentDictionary<int, ConcurrentBag<byte[]>> _writeBufferPool = new();

    public TransceiverSocketProtocol(IMessageProcessor messageProcessor,
        ISocketFactory socketFactory,
        ISerializer serializer,
        ILogger logger,
        IOptions<TransceiverConfiguration> configuration) : base(messageProcessor, serializer, logger, configuration)
    {
        _socketFactory = socketFactory;
        _setupWrite = new(() => Task.FromResult(_socketFactory.Connect()));
    }

    public sealed override Task<Socket> SetupWriterAsync(CancellationToken cancellationToken)
    {
        return _setupWrite.Value;
    }

    protected sealed override byte[] ToDataArray<T>(T data)
    {
        TransceiverMessage message = new(data, Serializer);
        int size = TransceiverHeader.Size + message.Data.Length;
        byte[] buffer = RentWriteBuffer(size);
        return message.ToBytes(buffer);
    }

    protected sealed override Task<(int, object)> ReadAsync(Socket reader, byte[] buffer, CancellationToken cancellationToken)
    {
        SocketAsyncEventArgs socketArgs = RentSocketArgs();
        socketArgs.SetBuffer(buffer, 0, buffer.Length);
        TaskCompletionSource<(int, object)> tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
        socketArgs.UserToken = tcs;

        bool pending;
        if (reader.ProtocolType == ProtocolType.Udp)
        {
            socketArgs.RemoteEndPoint = _socketFactory.Listen().LocalEndPoint;
            pending = reader.ReceiveFromAsync(socketArgs);
        }
        else
        {
            pending = reader.ReceiveAsync(socketArgs);
        }

        if (!pending)
        {
            (int, object) result = (socketArgs.BytesTransferred, socketArgs.RemoteEndPoint!);
            ReturnSocketArgs(socketArgs);
            return Task.FromResult(result);
        }
        return tcs.Task;
    }

    protected sealed override Task<Socket> SetupReadAsync(CancellationToken cancellationToken)
    {
        Socket listenSocket = _socketFactory.Listen();
        return listenSocket.TryAcceptAsync();
    }

    protected sealed override Task WriteAsync(Socket transceiver, object client, byte[] data, CancellationToken cancellationToken)
    {
        SocketAsyncEventArgs socketArgs = RentSocketArgs();
        socketArgs.SetBuffer(data, 0, data.Length);
        socketArgs.RemoteEndPoint = (EndPoint)client;
        TaskCompletionSource<object> tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
        socketArgs.UserToken = tcs;

        bool pending;
        if (client is null)
        {
            pending = transceiver.SendAsync(socketArgs);
        }
        else
        {
            pending = transceiver.SendToAsync(socketArgs);
        }
        if (pending)
        {
            return tcs.Task;
        }
        ReturnSocketArgs(socketArgs);
        ReturnWriteBuffer(data);
        return Task.CompletedTask;
    }

    private SocketAsyncEventArgs RentSocketArgs()
    {
        if (_socketArgsPool.TryTake(out SocketAsyncEventArgs? socketArgs))
        {
            return socketArgs;
        }
        socketArgs = new SocketAsyncEventArgs();
        socketArgs.Completed += OnSocketAsyncCompleted;
        return socketArgs;
    }

    private void ReturnSocketArgs(SocketAsyncEventArgs socketArgs)
    {
        socketArgs.UserToken = null;
        socketArgs.RemoteEndPoint = null;
        socketArgs.SetBuffer(null, 0, 0);
        _socketArgsPool.Add(socketArgs);
    }

    private byte[] RentWriteBuffer(int size)
    {
        if (_writeBufferPool.TryGetValue(size, out ConcurrentBag<byte[]>? bag) && bag.TryTake(out byte[]? buffer))
        {
            return buffer;
        }
        return new byte[size];
    }

    private void ReturnWriteBuffer(byte[] buffer)
    {
        ConcurrentBag<byte[]> bag = _writeBufferPool.GetOrAdd(buffer.Length, _ => []);
        bag.Add(buffer);
    }

    private void OnSocketAsyncCompleted(object? sender, SocketAsyncEventArgs args)
    {
        object? state = args.UserToken;
        int bytesTransferred = args.BytesTransferred;
        object remoteEndPoint = (object)args.RemoteEndPoint!;
        byte[]? writeBuffer = state is TaskCompletionSource<object> ? args.Buffer : null;
        ReturnSocketArgs(args);
        if (writeBuffer != null)
        {
            ReturnWriteBuffer(writeBuffer);
        }

        if (state is TaskCompletionSource<(int, object)> readTcs)
        {
            readTcs.SetResult((bytesTransferred, remoteEndPoint));
        }
        else if (state is TaskCompletionSource<object> writeTcs)
        {
            writeTcs.SetResult(default!);
        }
    }
}