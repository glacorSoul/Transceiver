// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using System.Text.Json.Serialization;

namespace Transceiver;

public class ProtocolTypeEnum : BetterEnum
{
    public static readonly ProtocolTypeEnum Channels = new(4, nameof(Channels));
    public static readonly ProtocolTypeEnum Ssl = new(5, nameof(Ssl));
    public static readonly ProtocolTypeEnum Tcp = new(3, nameof(Tcp));
    public static readonly ProtocolTypeEnum Udp = new(2, nameof(Udp));

    [JsonConstructor]
    public ProtocolTypeEnum(int value, string name) : base(value, name)
    {
    }
}