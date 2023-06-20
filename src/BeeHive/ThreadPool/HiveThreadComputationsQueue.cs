using System.Collections.Concurrent;

namespace BeeHive;

internal class HiveThreadComputationsQueue : BlockingCollection<Action>
{
    public HiveThreadComputationsQueue() : base(new ConcurrentQueue<Action>()) { }
}