// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Transceiver;

[DebuggerDisplay("{Id}")]
public struct TransceiverHelloIdentifier
{
    public TransceiverHelloIdentifier()
    {
        Id = Guid.NewGuid();
    }

    [JsonConstructor]
    public TransceiverHelloIdentifier(Guid id)
    {
        Id = id;
    }

    public Guid Id { get; set; }

    public override readonly bool Equals(object? obj)
    {
        if (obj is not TransceiverHelloIdentifier other)
        {
            return false;
        }
        return other.Id == Id;
    }

    public override readonly int GetHashCode()
    {
        return Id.GetHashCode();
    }
}