namespace BeeHive;

public class HiveBuilder<TRequest, TResult>
{
    private readonly Compute<TRequest, TResult> _compute;
    private Func<HiveConfiguration, HiveConfiguration>? _configure;

    public HiveBuilder(Compute<TRequest, TResult> compute) => _compute = compute;

    public HiveBuilder<TRequest, TResult> WithConfiguration(Func<HiveConfiguration, HiveConfiguration>? configure)
    {
        _configure = configure;
        return this;
    }

    public Hive<TRequest, TResult> Build()
    {
        var defaultConfiguration = HiveConfiguration.Default;
        var configuration = _configure?.Invoke(defaultConfiguration) ?? defaultConfiguration;

        return new Hive<TRequest, TResult>(_compute, configuration);
    }
}