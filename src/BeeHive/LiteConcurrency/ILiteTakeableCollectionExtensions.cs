using System.Diagnostics.CodeAnalysis;

namespace BeeHive;

public static class ILiteTakeableCollectionExtensions
{
    private const int Infinite = -1;

    public static bool TryTakeOrWait<TItem>(this ILiteTakeableCollection<TItem> takeable, int waitMilliseconds, [MaybeNullWhen(false)] out TItem item)
    {
        return takeable.TryTakeOrWait(waitMilliseconds, CancellationToken.None, out item);
    }

    public static bool TryTakeOrWait<TItem>(
        this ILiteTakeableCollection<TItem> takeable,
        CancellationToken cancellationToken,
        [MaybeNullWhen(false)] out TItem item)
    {
        return takeable.TryTakeOrWait(waitMilliseconds: Infinite, cancellationToken, out item);
    }

    public static bool TryTakeOrWait<TItem>(this ILiteTakeableCollection<TItem> takeable, [MaybeNullWhen(false)] out TItem item)
    {
        return takeable.TryTakeOrWait(waitMilliseconds: Infinite, CancellationToken.None, out item);
    }
}