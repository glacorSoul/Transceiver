// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using System.Net;
using System.Net.Sockets;

namespace Transceiver;

public sealed class TcpSocketFactory : SocketFactory
{
    public TcpSocketFactory(string server) : this(Parse(server))
    {
    }

    public TcpSocketFactory(IPEndPoint server) : base(server)
    {
    }

    protected override Socket Connect(object factoryIdentifier)
    {
        IPEndPoint serverEndPoint = Parse(factoryIdentifier.ToString()!);

        Socket tcpSocket = new(serverEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        tcpSocket.Connect(serverEndPoint);
        return tcpSocket;
    }

    protected override Socket Listen(object factoryIdentifier)
    {
        IPEndPoint serverEndPoint = Parse(factoryIdentifier.ToString()!);

        Socket tcpSocket = new(serverEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        tcpSocket.Bind(serverEndPoint);
        tcpSocket.Listen(100);
        return tcpSocket;
    }
}