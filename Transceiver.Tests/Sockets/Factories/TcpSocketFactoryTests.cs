// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using System.Net;
using System.Net.Sockets;

namespace Transceiver.Tests.Sockets.Factories;

[Trait("Category", "SocketFactories")]
public class TcpSocketFactoryTests
{
    [Fact]
    public void Connect_ShouldReturnConnectedSocket()
    {
        IPEndPoint server = new(IPAddress.Parse("127.0.0.1"), 8000);
        TcpSocketFactory factory = new(server);

        _ = factory.Listen();
        Socket socket = factory.Connect();

        Assert.NotNull(socket);
        Assert.Equal(SocketType.Stream, socket.SocketType);
        Assert.Equal(ProtocolType.Tcp, socket.ProtocolType);
    }

    [Fact]
    public void Constructor_WithIPEndPoints_ShouldInitializeFactoryIdentifier()
    {
        IPEndPoint server = new(IPAddress.Parse("127.0.0.1"), 8000);

        TcpSocketFactory factory = new(server);

        Assert.Equal($"{server}", factory.FactoryIdentifier);
    }

    [Fact]
    public void Constructor_WithStrings_ShouldInitializeFactoryIdentifier()
    {
        string server = "127.0.0.1:6000";

        TcpSocketFactory factory = new(server);

        Assert.Equal($"{server}", factory.FactoryIdentifier);
    }

    [Fact]
    public void Listen_ShouldReturnListeningSocket()
    {
        IPEndPoint server = new(IPAddress.Parse("127.0.0.1"), 8000);
        TcpSocketFactory factory = new(server);

        Socket socket = factory.Listen();

        Assert.NotNull(socket);
        Assert.Equal(SocketType.Stream, socket.SocketType);
        Assert.Equal(ProtocolType.Tcp, socket.ProtocolType);
    }
}