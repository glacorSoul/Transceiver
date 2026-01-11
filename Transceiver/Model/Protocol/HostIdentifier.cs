// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using System.Net;

namespace Transceiver;

public class HostIdentifier
{
    public HostIdentifier(IPEndPoint endpoint, string name, ProtocolTypeEnum protocolType)
    {
        Name = name;
        Address = new Uri($"{protocolType.Name.ToLowerInvariant()}:\\{endpoint.Address}:{endpoint.Port}");
    }

    public Uri Address { get; private set; }

    public string Name { get; private set; }

    public ProtocolTypeEnum ProtocolType
    {
        get
        {
            return ProtocolTypeEnum.FromScheme(Address.Scheme);
        }
    }

    public IPEndPoint ToEndPoint()
    {
        return ToEndPoints()[0];
    }

    public IPEndPoint[] ToEndPoints()
    {
        IPAddress[] addresses = Dns.GetHostAddresses(Address.Host);
        if (addresses.Length == 0)
        {
            throw new InvalidOperationException($"Could not resolve host '{Address.Host}' to an IP address.");
        }
        return [.. addresses.Select(a => new IPEndPoint(a, Address.Port))];
    }
}