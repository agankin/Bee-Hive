namespace BeeHive;

internal static class EnumerableExtensions
{
    public static void ForEach<TItem>(this IEnumerable<TItem> items, Action<TItem> handle)
    {
        foreach (var item in items)
            handle(item);
    }
}