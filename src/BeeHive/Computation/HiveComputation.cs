namespace BeeHive;

internal readonly struct HiveComputation
{
    private readonly Action _compute;

    public HiveComputation(Action compute)
    {
        _compute = compute;
    }

    internal void Invoke() => _compute();
}