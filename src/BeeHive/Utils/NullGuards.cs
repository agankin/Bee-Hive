namespace BeeHive;

internal static class NullGuards
{
    public static TValue NotNull<TValue>(this TValue? value, string? valueName = null) =>
        value ?? throw new Exception($"{valueName.NotEmptyOr("Value")} is null.");

    public static TValue ArgNotNull<TValue>(this TValue? value, string? argName = null) =>
        value ?? throw new ArgumentNullException(argName);

    private static string NotEmptyOr(this string? str, string alternative) =>
        string.IsNullOrEmpty(str) ? alternative : str;
}