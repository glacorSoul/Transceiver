// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

namespace Transceiver;

/// <summary>
/// Context information for deferred transceiver startup after service provider initialization.
/// </summary>
public class TransceiverStartupContext
{
    public CancellationToken CancellationToken { get; set; }
    public bool ServerOnly { get; set; }
    public Type ProcessorType { get; set; } = null!;
}
