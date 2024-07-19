using System.Diagnostics.CodeAnalysis;

namespace BeeHive;

public interface IBlockingReadOnlyCollection<TItem> : IEnumerable<TItem>
{
    public int Count { get; }

    public bool TryTakeOrWait(int waitMilliseconds, CancellationToken cancellationToken, [MaybeNullWhen(false)] out TItem item);
}