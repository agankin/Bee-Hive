using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace BeeHive;

internal class LiteConcurrentQueue<TItem> : IEnumerable<TItem>
{
    private readonly EqualityComparer<TItem> _itemComparer;

    private readonly LiteSpinLock _spinLock = new();

    private volatile QueueItem _head = null!;
    private volatile QueueItem _last = null!;
    private volatile int _count;

    public LiteConcurrentQueue() => _itemComparer = EqualityComparer<TItem>.Default;

    public LiteConcurrentQueue(EqualityComparer<TItem> itemComparer) => _itemComparer = itemComparer;

    public int Count => _count;

    public void Enqueue(TItem item)
    {
        var queueItem = new QueueItem(item);

        _spinLock.Lock(() =>
        {
            if (_head == null)
            {
                _head = _last = queueItem;
            }
            else
            {
                _last = _last.Next = queueItem;
            }

            _count++;
        });
    }

    public bool TryDequeue([MaybeNullWhen(false)] out TItem item)
    {
        bool hasItem;
        
        (hasItem, item) = _spinLock.Lock(() =>
        {
            if (_head == null)
                return (false, default(TItem));

            var item = _head.Item;
            if ((_head = _head.Next) == null)
                _last = null!;

            _count--;

            return (true, item);
        });

        return hasItem;
    }

    public void Remove(TItem item)
    {
        _spinLock.Lock(() =>
        {
            if (_head == null)
                return;

            if (_itemComparer.Equals(_head.Item, item))
            {
                _head = _head.Next;
                _count--;

                return;
            }

            var current = _head;
            while (current.Next != null && !_itemComparer.Equals(current.Next.Item, item))
                current = current.Next;

            if (current.Next == null)
                return;

            current.Next = current.Next.Next;
            _count--;
        });
    }

    public IEnumerator<TItem> GetEnumerator() => GetSnapshot().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private IEnumerable<TItem> GetSnapshot()
    {
        var snapshot = new List<TItem>();
        var current = _head;
        
        _spinLock.Lock(() =>
        {
            while (current != null)
            {
                snapshot.Add(current.Item);
                current = current.Next;
            }
        });

        return snapshot;
    }

    private record QueueItem(TItem Item)
    {
        private volatile QueueItem _next = null!;

        public QueueItem Next { get => _next; set => _next = value; }
    }
}