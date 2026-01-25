// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using System.Collections.Concurrent;

namespace Transceiver;

public sealed class ConcurrentDictionaryList<Tkey, TValue>
    where Tkey : notnull
    where TValue : class
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