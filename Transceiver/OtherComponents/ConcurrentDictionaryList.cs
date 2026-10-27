// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY
using System.Collections.Concurrent;

namespace Transceiver;

public interface IPredicate<in T>
{
    bool Matches(T value);
}

public sealed class ConcurrentDictionaryList<Tkey, TValue>
    where Tkey : notnull
    where TValue : notnull
{
    private sealed class Bag
    {
        public readonly List<TValue> Items = [];
    }

    private readonly ConcurrentDictionary<Tkey, Bag> _dictionary = [];

    public void Add(Tkey key, TValue value)
    {
        Bag bag = _dictionary.GetOrAdd(key, static _ => new Bag());
        lock (bag)
        {
            bag.Items.Add(value);
        }
    }

    public async Task WaitForKey(Tkey key, CancellationToken cancellationToken)
    {
        while (!_dictionary.ContainsKey(key))
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Yield();
        }
    }

    public List<TValue> GetAndRemoveAll<TPredicate>(Tkey getKey, Tkey removeKey, TPredicate predicate)
        where TPredicate : struct, IPredicate<TValue>
    {
        return GetAndRemoveAll(getKey, removeKey, predicate, []);
    }

    /// <summary>
    /// Same as <see cref="GetAndRemoveAll{TPredicate}(Tkey, Tkey, TPredicate)"/> but reuses the supplied
    /// <paramref name="result"/> list instead of allocating a new one. The list is cleared before use.
    /// Callers that reuse the same list across calls must not use it concurrently.
    /// </summary>
    public List<TValue> GetAndRemoveAll<TPredicate>(Tkey getKey, Tkey removeKey, TPredicate predicate, List<TValue> result)
        where TPredicate : struct, IPredicate<TValue>
    {
        result.Clear();
        if (_dictionary.TryGetValue(getKey, out Bag? bag))
        {
            lock (bag)
            {
                foreach (TValue item in bag.Items)
                {
                    if (predicate.Matches(item))
                    {
                        result.Add(item);
                    }
                }
            }
        }

        if (_dictionary.TryGetValue(removeKey, out Bag? bag2))
        {
            bool isEmpty;
            lock (bag2)
            {
                List<TValue> items = bag2.Items;
                int writeIndex = 0;
                for (int i = 0; i < items.Count; i++)
                {
                    TValue item = items[i];
                    if (predicate.Matches(item))
                    {
                        result.Add(item);
                    }
                    else
                    {
                        items[writeIndex++] = item;
                    }
                }

                if (writeIndex != items.Count)
                {
                    items.RemoveRange(writeIndex, items.Count - writeIndex);
                }

                isEmpty = items.Count == 0;
            }

            if (isEmpty)
            {
                RemoveBagIfEmpty(removeKey, bag2);
            }
        }
        return result;
    }

    private void RemoveBagIfEmpty(Tkey key, Bag bag)
    {
        lock (bag)
        {
            if (bag.Items.Count == 0)
            {
                _ = ((ICollection<KeyValuePair<Tkey, Bag>>)_dictionary).Remove(new KeyValuePair<Tkey, Bag>(key, bag));
            }
        }
    }

    /// <summary>
    /// Returns every value matching <paramref name="predicate"/> from both the <paramref name="getKey"/>
    /// and <paramref name="removeKey"/> bags, without removing anything. Use this for subscribers whose
    /// lifetime is managed independently (e.g. via <see cref="RemoveWhere{TPredicate}(Tkey, TPredicate)"/>).
    /// The supplied <paramref name="result"/> list is cleared before use.
    /// </summary>
    public List<TValue> GetAll<TPredicate>(Tkey getKey, Tkey removeKey, TPredicate predicate, List<TValue> result)
        where TPredicate : struct, IPredicate<TValue>
    {
        result.Clear();
        if (_dictionary.TryGetValue(getKey, out Bag? bag))
        {
            lock (bag)
            {
                foreach (TValue item in bag.Items)
                {
                    if (predicate.Matches(item))
                    {
                        result.Add(item);
                    }
                }
            }
        }

        if (!EqualityComparer<Tkey>.Default.Equals(getKey, removeKey) &&
            _dictionary.TryGetValue(removeKey, out Bag? bag2))
        {
            lock (bag2)
            {
                foreach (TValue item in bag2.Items)
                {
                    if (predicate.Matches(item))
                    {
                        result.Add(item);
                    }
                }
            }
        }
        return result;
    }

    public List<TValue> GetAll(Tkey key, Predicate<TValue> condition)
    {
        List<TValue> result = [];
        if (_dictionary.TryGetValue(key, out Bag? bag))
        {
            lock (bag)
            {
                foreach (TValue item in bag.Items)
                {
                    if (condition(item))
                    {
                        result.Add(item);
                    }
                }
            }
        }
        return result;
    }

    /// <summary>
    /// Removes every value matching <paramref name="predicate"/> from the bag associated with <paramref name="key"/>.
    /// Returns true if at least one value was removed. The bag is removed from the dictionary if it becomes empty.
    /// </summary>
    public bool RemoveWhere<TPredicate>(Tkey key, TPredicate predicate)
        where TPredicate : struct, IPredicate<TValue>
    {
        if (!_dictionary.TryGetValue(key, out Bag? bag))
        {
            return false;
        }

        bool removedAny;
        bool isEmpty;
        lock (bag)
        {
            List<TValue> items = bag.Items;
            int writeIndex = 0;
            removedAny = false;
            for (int i = 0; i < items.Count; i++)
            {
                TValue item = items[i];
                if (predicate.Matches(item))
                {
                    removedAny = true;
                }
                else
                {
                    items[writeIndex++] = item;
                }
            }

            if (writeIndex != items.Count)
            {
                items.RemoveRange(writeIndex, items.Count - writeIndex);
            }

            isEmpty = items.Count == 0;
        }

        if (isEmpty)
        {
            RemoveBagIfEmpty(key, bag);
        }
        return removedAny;
    }
}