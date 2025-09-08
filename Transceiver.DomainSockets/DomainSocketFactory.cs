// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using System;
using System.Linq;
using System.Net.Sockets;

namespace Transceiver.DomainSockets;

public class DomainSocketFactory : SocketFactory
{
    private static readonly Random Random = new();

    public DomainSocketFactory() : this(GenerateRandomPath())
    {
    }

    public DomainSocketFactory(string path) : base(path)
    {
        Path = path;
    }

    public string Path { get; set; }

    public static string GenerateRandomPath()
    {
        return GenerateRandomPath(10);
    }

    public static string GenerateRandomPath(int size)
    {
        string chars = "abcdefghijklmnopqrstuvwxyz";
        char[] pathChars = [.. Enumerable.Range(0, size).Select(_ => chars[Random.Next(chars.Length)])];
        return new string(pathChars);
    }

    protected override Socket Connect(object factoryIdentifier)
    {
        string path = factoryIdentifier.ToString()!;
        UnixDomainSocketEndPoint endPoint = new(path);
        Socket socket = new(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
        socket.Connect(endPoint);
        return socket;
    }

    protected override Socket Listen(object factoryIdentifier)
    {
        string path = factoryIdentifier.ToString()!;
        UnixDomainSocketEndPoint endPoint = new(path);
        Socket socket = new(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
        socket.Bind(endPoint);
        socket.Listen(100);
        return socket;
    }
}