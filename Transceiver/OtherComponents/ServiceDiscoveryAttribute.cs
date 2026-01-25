// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

namespace Transceiver;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class ServiceDiscoveryAttribute : Attribute
{
    public ServiceDiscoveryAttribute(string url)
    {
        Url = url;
    }

    public string Url { get; }
}