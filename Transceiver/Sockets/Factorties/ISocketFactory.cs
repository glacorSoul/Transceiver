// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using System.Net.Sockets;

namespace Transceiver;

public interface ISocketFactory
{
    Task<Socket> AcceptAsync(Socket listenSocket);

    Socket Connect();

    Socket Listen();
}