// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

namespace Transceiver;

public interface ISerializer
{
    object Deserialize(Type type, byte[] data);

    T Deserialize<T>(byte[] data);

    Task<object> DeserializeAsync(Type type, byte[] data, CancellationToken cancellationToken);

    Task<T> DeserializeAsync<T>(byte[] data, CancellationToken cancellationToken);

    byte[] Serialize<T>(T data);

    Task<byte[]> SerializeAsync(Type type, object data, CancellationToken cancellationToken);

    Task<byte[]> SerializeAsync<T>(T data, CancellationToken cancellationToken);

    byte[] Seriazlize(Type type, object data);
}