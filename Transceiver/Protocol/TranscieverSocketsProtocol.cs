// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Sockets;

namespace Transceiver;

public sealed class TransceiverSocketProtocol : ReceiveMessagesProtocol<Socket>
{
    private readonly Lazy<Task<Socket>> _setupWrite;
    private readonly ISocketFactory _socketFactory;

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

    protected sealed override Task<(int, object)> ReadAsync(Socket reader, byte[] buffer, CancellationToken cancellationToken)
    {
        TaskCompletionSource<(int, object)> tcs = new();
        SocketAsyncEventArgs socketArgs = new();
        socketArgs.SetBuffer(buffer, 0, buffer.Length);
        socketArgs.Completed += (sender, args) =>
        {
            tcs.SetResult((args.BytesTransferred, args.RemoteEndPoint!));
        };
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
            return Task.FromResult((socketArgs.BytesTransferred, (object)socketArgs.RemoteEndPoint!));
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
        TaskCompletionSource<object> tcs = new();
        SocketAsyncEventArgs socketArgs = new();
        socketArgs.SetBuffer(data, 0, data.Length);
        socketArgs.RemoteEndPoint = (EndPoint)client;
        socketArgs.Completed += (sender, args) =>
        {
            tcs.SetResult(default!);
        };
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
        return Task.CompletedTask;
    }
}