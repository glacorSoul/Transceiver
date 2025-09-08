// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using Microsoft.Extensions.DependencyInjection;

namespace Transceiver;

internal sealed class DirectProtocolSetup : CustomProtocolSetup
{
    public DirectProtocolSetup(Type transceiverType, IServiceCollection services) : base(transceiverType, typeof(DirectProtocol), services)
    {
    }
}