// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

namespace Transceiver.Requests;

public class ServiceDiscoveryModel
{
    public string RequestName { get; set; } = string.Empty;
    public Dictionary<string, object> RequestProperties { get; set; } = [];
    public string ResponseName { get; set; } = string.Empty;
    public Dictionary<string, object> ResponseProperties { get; set; } = [];
}