// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using System.Net.Sockets;

namespace Transceiver;

internal static class SocketUtils
{
    public static async Task<Socket> TryAcceptAsync(this Socket socket)
    {
        try
        {
            return socket.SocketType switch
            {
                SocketType.Dgram => socket,
                SocketType.Unknown => socket,
                SocketType.Stream => await socket.AcceptAsync(),
                SocketType.Raw => socket,
                SocketType.Rdm => socket,
                SocketType.Seqpacket => socket,
                _ => await socket.AcceptAsync()
            };
        }
        catch (InvalidOperationException)
        {
            return socket;
        }
    }
}