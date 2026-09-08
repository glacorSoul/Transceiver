// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using System.Collections.Concurrent;

namespace Transceiver;

/// <summary>
/// Default implementation of IListenerManager that tracks and manages background listener tasks
/// </summary>
public class ListenerManager : IListenerManager
{
    private readonly ConcurrentBag<Task> _listenerTasks = [];

    /// <summary>
    /// Registers a listener task
    /// </summary>
    public void RegisterListener(Task listenerTask)
    {
        _listenerTasks.Add(listenerTask);
    }

    /// <summary>
    /// Waits for all registered listeners to complete
    /// </summary>
    public async Task WaitForAllListenersAsync()
    {
        if (_listenerTasks.IsEmpty)
        {
            return;
        }

        try
        {
            await Task.WhenAll(_listenerTasks);
        }
        catch (OperationCanceledException)
        {
            // Expected when cancellation is requested
        }
        catch (ObjectDisposedException)
        {
            // Expected when sockets are closed
        }
    }
}
