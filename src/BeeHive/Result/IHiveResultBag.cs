namespace BeeHive;

public interface IHiveResultBag<TRequest, TResult> : ILiteTakeableCollection<Result<TRequest, TResult>>, IEnumerable<Result<TRequest, TResult>>, IDisposable
{
}