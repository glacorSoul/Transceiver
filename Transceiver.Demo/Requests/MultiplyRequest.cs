// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using MessagePack;

namespace Transceiver.Demo;

[MessagePackObject]
public class MultiplyRequest
{
    [Key(0)]
    public int A { get; set; }
    [Key(1)]
    public int B { get; set; }
}

[MessagePackObject]
public class MultiplyResponse
{
    [Key(0)]
    public int Result { get; set; }
}
