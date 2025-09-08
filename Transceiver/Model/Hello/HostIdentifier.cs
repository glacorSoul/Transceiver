// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using System.Diagnostics;

namespace Transceiver;

[DebuggerDisplay("{EndPoint}")]
public class HostIdentifier
{
    public HostIdentifier(string endPoint, ProtocolTypeEnum protocolType)
    {
        EndPoint = endPoint;
        ProtocolType = protocolType;
    }

    public string EndPoint { get; set; }
    public ProtocolTypeEnum ProtocolType { get; set; }
}