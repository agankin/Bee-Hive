using System.Diagnostics.CodeAnalysis;

namespace BeeHive;

/// <summary>
/// Represents a collection supporting taking out elements.
/// </summary>
/// <typeparam name="TItem">The type of elements.</typeparam>
public interface ITakeableCollection<TItem>
{
    /// <summary>
    /// Contains the number of elements in this collection.
    /// </summary>
    public int Count { get; }

    /// <summary>
    /// Tries taking out an element.
    /// If an element exists the call removes it and returns in output parameter. 
    /// </summary>
    /// <param name="item">An element.</param>
    /// <returns>True if an element found otherwise false.</returns>
    public bool TryTake([MaybeNullWhen(false)] out TItem item);

    /// <summary>
    /// Tries taking out an element.
    /// If an element exists the call removes it and returns in output parameter.
    /// If there are no elements the call waits for a new element added.
    /// </summary>
    /// <param name="waitMilliseconds">
    /// The maximum period of time in milliseconds for waiting a new element added. For infinite value pass -1.
    /// </param>
    /// <param name="cancellationToken">A cancellation token for cancelling waiting for a new element added.</param>
    /// <param name="item">An element.</param>
    /// <returns>True if an element found otherwise false.</returns>
    public bool TryTakeOrWait(int waitMilliseconds, CancellationToken cancellationToken, [MaybeNullWhen(false)] out TItem item);
}