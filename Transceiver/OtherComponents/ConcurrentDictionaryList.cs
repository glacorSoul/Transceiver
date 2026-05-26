// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY
using System.Collections.Concurrent;

namespace Transceiver;

public sealed class ConcurrentDictionaryList<Tkey, TValue>
    where Tkey : notnull
    where TValue : notnull
{
    private readonly ConcurrentDictionary<Tkey, ConcurrentBag<TValue>> _dictionary = [];

    public void Add(Tkey key, TValue value)
    {
        _ = _dictionary.AddOrUpdate(key, key => [value], (key, bag) =>
        {
            bag.Add(value);
            return bag;
        });
    }

    public async Task WaitForKey(Tkey key, CancellationToken cancellationToken)
    {
        while (!_dictionary.ContainsKey(key))
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Yield();
        }
    }

    private void RemoveElement(Tkey key, TValue value, List<TValue> result)
    {
        if (_dictionary.TryGetValue(key, out ConcurrentBag<TValue>? bag))
        {
            TValue[] items = [.. bag];
            result.AddRange(items.Where(e => e.Equals(value)));
            TValue[] kept = [.. items.Where(e => !e.Equals(value))];
            if (kept.Length == 0)
            {
                _ = _dictionary.TryRemove(key, out _);
            }
            else
            {
                ConcurrentBag<TValue> newBag = [.. kept];
                _ = _dictionary.TryUpdate(key, newBag, bag);
            }
        }
    }

    public List<TValue> GetAndRemoveAll(Tkey getKey, Tkey removeKey, Predicate<TValue> predicate)
    {
        List<TValue> result = [];
        if (_dictionary.TryGetValue(getKey, out ConcurrentBag<TValue>? bag))
        {
            foreach (TValue item in bag)
            {
                if (predicate(item))
                {
                    result.Add(item);
                }
            }
        }

        if (_dictionary.TryGetValue(removeKey, out ConcurrentBag<TValue>? bag2))
        {
            foreach (TValue item in bag2)
            {
                if (predicate(item))
                {
                    RemoveElement(removeKey, item, result);
                }
            }
        }
        return result;
    }

    public List<TValue> GetAll(Tkey key, Predicate<TValue> condition)
    {
        List<TValue> result = [];
        if (_dictionary.TryGetValue(key, out ConcurrentBag<TValue>? bag))
        {
            foreach (TValue item in bag)
            {
                if (condition(item))
                {
                    result.Add(item);
                }
            }
        }
        return result;
    }

    public List<TValue> RemoveAll(Tkey key, Predicate<TValue> condition)
    {
        List<TValue> removedResult = [];

        while (true)
        {
            if (!_dictionary.TryGetValue(key, out ConcurrentBag<TValue>? currentBag))
            {
                // no bag present
                return removedResult;
            }

            TValue[] items = [.. currentBag];
            TValue[] toRemove = [.. items.Where(e => condition(e))];

            if (toRemove.Length == 0)
            {
                // nothing to remove
                return removedResult;
            }

            TValue[] kept = [.. items.Where(e => !condition(e))];

            if (kept.Length == 0)
            {
                if (_dictionary.TryRemove(key, out _))
                {
                    removedResult.AddRange(toRemove);
                    return removedResult;
                }
                continue;
            }
            else
            {
                ConcurrentBag<TValue> newBag = [.. kept];
                if (_dictionary.TryUpdate(key, newBag, currentBag))
                {
                    removedResult.AddRange(toRemove);
                    return removedResult;
                }
                continue;
            }
        }
    }
}