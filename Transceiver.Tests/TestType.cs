// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

namespace Transceiver.Tests.Reflection;

public enum TestCases
{
    Value1,
    Value2,
    Value3
}

public class TestType
{
    public bool Boolean { get; set; }
    public byte Byte { get; set; }
    public char Char { get; set; }
    public TestType Child { get; set; } = default!;
    public DateTime DateTime { get; set; }
    public DateTimeOffset DateTimeOffset { get; set; }
    public decimal Decimal { get; set; }
    public double Double { get; set; }
    public TestCases Enum { get; set; }
    public float Float { get; set; }
    public Guid Guid { get; set; }
    public short Int16 { get; set; }
    public int Int32 { get; set; }
    public long Int64 { get; set; }
    public List<TestType> ListOfTestTypes { get; set; } = default!;
    public bool? NullableBoolean { get; set; }
    public byte? NullableByte { get; set; }
    public char? NullableChar { get; set; }
    public DateTime? NullableDateTime { get; set; }
    public DateTimeOffset? NullableDateTimeOffset { get; set; }
    public decimal? NullableDecimal { get; set; }
    public double? NullableDouble { get; set; }
    public TestCases? NullableEnum { get; set; }
    public float? NullableFloat { get; set; }
    public Guid? NullableGuid { get; set; }
    public short? NullableInt16 { get; set; }
    public int? NullableInt32 { get; set; }
    public long? NullableInt64 { get; set; } = default!;
    public sbyte? NullableSByte { get; set; }
    public TimeSpan? NullableTimeSpan { get; set; }
    public ushort? NullableUInt16 { get; set; }
    public uint? NullableUInt32 { get; set; }
    public ulong? NullableUInt64 { get; set; }
    public sbyte SByte { get; set; }
    public string Text { get; set; } = default!;
    public TimeSpan TimeSpan { get; set; }
    public ushort UInt16 { get; set; }
    public uint UInt32 { get; set; }
    public ulong UInt64 { get; set; }
}