// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Transceiver;

[DebuggerDisplay("({Value}): {Name}")]
public abstract class BetterEnum
{
    protected BetterEnum()
    {
        Value = int.MinValue;
        Name = string.Empty;
    }

    [JsonConstructor]
    protected BetterEnum(int value, string name)
    {
        Value = value;
        Name = name;
    }

    public string Name { get; set; }
    public int Value { get; set; }

    public override bool Equals(object? obj)
    {
        if (obj is not BetterEnum other)
        {
            return false;
        }
        return other.Value == Value;
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
}