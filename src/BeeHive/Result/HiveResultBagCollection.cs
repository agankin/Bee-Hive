using System.Collections;

namespace BeeHive;

internal class HiveResultBagCollection<TRequest, TResult> : IEnumerable<HiveResultBag<TRequest, TResult>>
{
    private readonly ConcurrentSet<HiveResultBag<TRequest, TResult>> _resultBagCollection = new();

    public IHiveResultBag<TRequest, TResult> AddNewBag()
    {
        var resultBag = new HiveResultBag<TRequest, TResult>(_resultBagCollection.Remove);
        _resultBagCollection.Add(resultBag);

        return resultBag;
    }

    public void AddResult(Result<TRequest, TResult> result)
    {
        if (_resultBagCollection.Count == 0)
            return;

        foreach (var resultBag in _resultBagCollection)
            resultBag.Add(result);
    }

    public IEnumerator<HiveResultBag<TRequest, TResult>> GetEnumerator() => _resultBagCollection.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}