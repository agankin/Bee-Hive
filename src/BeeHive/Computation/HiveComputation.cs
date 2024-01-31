namespace BeeHive;

internal readonly struct HiveComputation
{
    public HiveComputation(Action compute)
    {
        Compute = compute;
    }

    public Action Compute { get; }
}