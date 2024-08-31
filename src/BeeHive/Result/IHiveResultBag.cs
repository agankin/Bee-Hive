namespace BeeHive;

/// <summary>
/// Represents a Hive result bag for storing computations results.
/// </summary>
/// <typeparam name="TRequest">The request type of the computation.</typeparam>
/// <typeparam name="TResult">The result type of the computation.</typeparam>
public interface IHiveResultBag<TRequest, TResult> : ITakeableCollection<Result<TRequest, TResult>>, IEnumerable<Result<TRequest, TResult>>, IDisposable
{
}