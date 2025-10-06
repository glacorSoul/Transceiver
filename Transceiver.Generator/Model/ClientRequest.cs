// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

namespace Transceiver.Generator;

public struct ClientRequest<TRequest>
{
    public TRequest Data { get; set; }
    public Guid Id { get; set; }
    public DateTimeOffset TimeStamp { get; set; }
}