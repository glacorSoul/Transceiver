// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using System.Net;
using System.Net.Sockets;

namespace Transceiver;

public sealed class UdpSocketFactory : SocketFactory
{
    public UdpSocketFactory(string server) : this(Parse(server))
    {
    }

    public UdpSocketFactory(IPEndPoint server) : base(server.ToString())
    {
    }

    protected override Socket Connect(object factoryIdentifier)
    {
        IPEndPoint serverEndPoint = Parse(factoryIdentifier.ToString()!);

        Socket udpSocket = new(serverEndPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
        udpSocket.Connect(serverEndPoint);
        return udpSocket;
    }

    protected override Socket Listen(object factoryIdentifier)
    {
        IPEndPoint serverEndPoint = Parse(factoryIdentifier.ToString()!);

        Socket udpSocket = new(serverEndPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
        udpSocket.Bind(serverEndPoint);
        return udpSocket;
    }
}