// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

namespace Transceiver;

public sealed class ConcurrentDictionaryList<Tkey, TValue>
    where Tkey : notnull
    where TValue : class
{
    private readonly Dictionary<Tkey, List<TValue>> _dictionary = [];

    public IEnumerable<KeyValuePair<Tkey, List<TValue>>> Entries
    {
        get
        {
            lock (_dictionary)
            {
                return [.. _dictionary.Select(e => new KeyValuePair<Tkey, List<TValue>>(e.Key, [.. e.Value]))];
            }
        }
    }

    public void Add(Tkey key, TValue value)
    {
        List<TValue> list;
        lock (_dictionary)
        {
            if (!_dictionary.TryGetValue(key, out list!))
            {
                list = [];
                _dictionary[key] = list;
            }
            list.Add(value);
            Monitor.PulseAll(_dictionary);
        }
    }

    public List<TValue> GetAll(Tkey key, Predicate<TValue> condition)
    {
        lock (_dictionary)
        {
            List<TValue> list;
            bool hasValue = _dictionary.TryGetValue(key, out list!);
            if (hasValue)
            {
                IEnumerable<TValue> matching = list.Where(e => condition(e));
                return [.. matching];
            }
            return [];
        }
    }

    public List<TValue> RemoveAll(Tkey key)
    {
        lock (_dictionary)
        {
            List<TValue> list;
            if (!_dictionary.TryGetValue(key, out list!))
            {
                return [];
            }
            _ = _dictionary.Remove(key);
            return list;
        }
    }

    public List<TValue> RemoveAll(Tkey key, Predicate<TValue> condition)
    {
        lock (_dictionary)
        {
            List<TValue> list;
            bool hasValue = _dictionary.TryGetValue(key, out list!);
            if (!hasValue)
            {
                return [];
            }
            List<TValue> matching = [.. list.Where(e => condition(e))];
            List<TValue> nonMatching = [.. list.Where(e => !condition(e))];
            if (nonMatching.Count == 0)
            {
                _ = _dictionary.Remove(key);
            }
            else
            {
                _dictionary[key] = nonMatching;
            }
            return matching;
        }
    }

    public List<TValue> WaitAndRemoveAll(Tkey key, CancellationToken cancellationToken)
    {
        lock (_dictionary)
        {
            List<TValue> list;
            while (!_dictionary.TryGetValue(key, out list!) && !cancellationToken.IsCancellationRequested)
            {
                _ = Monitor.Wait(_dictionary, 10);
            }
            _ = _dictionary.Remove(key);
            return list;
        }
    }

    public TValue WaitAndRemoveSingle(Tkey key, Predicate<TValue> condition, CancellationToken cancellationToken)
    {
        lock (_dictionary)
        {
            List<TValue> list;
            while (!cancellationToken.IsCancellationRequested)
            {
                bool hasValue = _dictionary.TryGetValue(key, out list!);
                if (hasValue)
                {
                    TValue? elem = list.FirstOrDefault(e => condition(e));
                    hasValue = hasValue && elem is not null;
                    if (hasValue)
                    {
                        _ = list.Remove(elem!);
                        return elem!;
                    }
                }
                _ = Monitor.Wait(_dictionary, 10);
            }
            return default!;
        }
    }
}