// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace Transceiver;

public abstract class SocketFactory : ISocketFactory
{
    private static readonly ConcurrentDictionary<object, SocketFactory> FactoryMap = [];

    private readonly Lazy<Task<Socket>> _acceptSocket;
    private readonly Lazy<Socket> _connectSocket;
    private readonly Lazy<Socket> _listenSocket;
    private object? _factoryIdentifier;

    protected SocketFactory(object factoryIdentifier)
    {
        FactoryIdentifier = factoryIdentifier;
        _listenSocket = new Lazy<Socket>(() => Listen(factoryIdentifier));
        _connectSocket = new Lazy<Socket>(() => Connect(factoryIdentifier));
        _acceptSocket = new Lazy<Task<Socket>>(AcceptAsync);
        _ = FactoryMap.GetOrAdd(FactoryIdentifier, this);
    }

    public object FactoryIdentifier
    {
        get { return _factoryIdentifier!; }
        set { _factoryIdentifier = ReEvaluateIdentifier(value); }
    }

    public static SocketFactory GetFactory(ProtocolTypeEnum protocolType, IPEndPoint server)
    {
        if (protocolType.Equals(ProtocolTypeEnum.Tcp))
        {
            return new TcpSocketFactory(server);
        }
        if (protocolType.Equals(ProtocolTypeEnum.Udp))
        {
            return new UdpSocketFactory(server);
        }
        if (protocolType.Equals(ProtocolTypeEnum.Ssl))
        {
            return new TcpSocketFactory(server);
        }
        throw new NotSupportedException($"Protocol type {protocolType} is not supported.");
    }

    public static SocketFactory GetFactory(object factoryIdentifier)
    {
        return FactoryMap[factoryIdentifier];
    }

    public Task<Socket> AcceptAsync(Socket listenSocket)
    {
        return FactoryMap[FactoryIdentifier]._acceptSocket.Value;
    }

    public Socket Connect()
    {
        return FactoryMap[FactoryIdentifier]._connectSocket.Value;
    }

    public Socket Listen()
    {
        return FactoryMap[FactoryIdentifier]._listenSocket.Value;
    }

    protected static IPEndPoint Parse(string server)
    {
        string host = server.Substring(0, server.IndexOf(":"));
        int port = int.Parse(server.Substring(server.IndexOf(":") + 1));
        return new IPEndPoint(IPAddress.Parse(host), port);
    }

    protected abstract Socket Connect(object factoryIdentifier);

    protected abstract Socket Listen(object factoryIdentifier);

    protected virtual object ReEvaluateIdentifier(object factoryIdentifier)
    {
        return factoryIdentifier.ToString()!;
    }

    private Task<Socket> AcceptAsync()
    {
        return _listenSocket.Value.TryAcceptAsync();
    }
}