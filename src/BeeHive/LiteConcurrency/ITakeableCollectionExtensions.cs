using System.Diagnostics.CodeAnalysis;

namespace BeeHive;

/// <summary>
/// Contains extension methods for <see cref="ITakeableCollection{TItem}"/>.
/// </summary>
public static class ITakeableCollectionExtensions
{
    private const int InfiniteTime = -1;

    /// <summary>
    /// Tries taking out an element.
    /// If an element exists the call removes it and returns in output parameter.
    /// If there are no elements the call waits for a new element added.
    /// </summary>
    /// <param name="waitMilliseconds">
    /// The maximum period of time in milliseconds for waiting a new element added. For infinite value pass -1.
    /// </param>
    /// <param name="item">An element.</param>
    /// <returns>True if an element found otherwise false.</returns>
    public static bool TryTakeOrWait<TItem>(this ITakeableCollection<TItem> takeable, int waitMilliseconds, [MaybeNullWhen(false)] out TItem item)
    {
        return takeable.TryTakeOrWait(waitMilliseconds, CancellationToken.None, out item);
    }

    /// <summary>
    /// Tries taking out an element.
    /// If an element exists the call removes it and returns in output parameter.
    /// If there are no elements the call waits infinitely for a new element added.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token for cancelling waiting for a new element added.</param>
    /// <param name="item">An element.</param>
    /// <returns>True if an element found otherwise false.</returns>
    public static bool TryTakeOrWait<TItem>(
        this ITakeableCollection<TItem> takeable,
        CancellationToken cancellationToken,
        [MaybeNullWhen(false)] out TItem item)
    {
        return takeable.TryTakeOrWait(waitMilliseconds: InfiniteTime, cancellationToken, out item);
    }

    /// <summary>
    /// Tries taking out an element.
    /// If an element exists the call removes it and returns in output parameter.
    /// If there are no elements the call waits infinitely for a new element added.
    /// </summary>
    /// <param name="item">An element.</param>
    /// <returns>True if an element found otherwise false.</returns>
    public static bool TryTakeOrWait<TItem>(this ITakeableCollection<TItem> takeable, [MaybeNullWhen(false)] out TItem item)
    {
        return takeable.TryTakeOrWait(waitMilliseconds: InfiniteTime, CancellationToken.None, out item);
    }
}