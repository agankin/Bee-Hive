namespace BeeHive;

/// <summary>
/// Represents a collection automatically populated with results of completed computations.
/// </summary>
/// <typeparam name="TRequest">The type of computation request.</typeparam>
/// <typeparam name="TResult">The type of computation result.</typeparam>
public interface IHiveResultBag<TRequest, TResult> : ITakeableCollection<Result<TRequest, TResult>>, IEnumerable<Result<TRequest, TResult>>, IDisposable
{
}