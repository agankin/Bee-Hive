using System.Collections;
using System.Collections.Concurrent;

namespace BeeHive;

internal class ConcurrentSet<TItem> : IEnumerable<TItem> where TItem : notnull
{
    private readonly ConcurrentDictionary<TItem, Nothing> _dict = new();

    public int Count => _dict.Keys.Count;

    public bool Contains(TItem item) => _dict.ContainsKey(item);

    public void Add(TItem item) => _dict.TryAdd(item, new Nothing());

    public void Remove(TItem item) => _dict.TryRemove(item, out _);

    public IEnumerator<TItem> GetEnumerator() => _dict.Keys.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}