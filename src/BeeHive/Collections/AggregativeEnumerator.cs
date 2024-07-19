using System.Collections;

namespace BeeHive;

internal class AggregativeEnumerator<TItem> : IEnumerator<TItem>
{
    private readonly IEnumerator<TItem>[] _enumerators;
    private int _current = 0;

    public AggregativeEnumerator(params IEnumerator<TItem>[] enumerators)
    {
        if (enumerators.Length == 0)
            throw new ArgumentException("The number of enumerators must be greater zero.", nameof(enumerators));

        _enumerators = enumerators;
    }

    public TItem Current => _enumerators[_current].Current;

    object? IEnumerator.Current => Current;

    public bool MoveNext()
    {
        var hasNext = _enumerators[_current].MoveNext();
        if (!hasNext && _current < _enumerators.Length - 1)
        {
            _current++;
            hasNext = _enumerators[_current].MoveNext();
        }

        return hasNext;
    }

    public void Reset()
    {
        _current = 0;
        foreach (var enumerator in _enumerators)
            enumerator.Reset();
    }

    public void Dispose()
    {
        foreach (var enumerator in _enumerators)
            enumerator.Dispose();
    }
}