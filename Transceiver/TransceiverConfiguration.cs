// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

namespace Transceiver;

public class TransceiverConfiguration
{
    public string CertificateThumbprint { get; set; } = default!;

    /// <summary>
    /// This value needs to be false for scenarios where the messages can remain persisted in disk and services are restarted.
    /// Can be set to false when application is not expected to be restarted.
    /// </summary>
    public bool OptimizeHelloSerialization { get; set; }

    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(30);
}