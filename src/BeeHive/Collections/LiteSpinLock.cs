namespace BeeHive;

public class LiteSpinLock
{
    private int _locked;

    public TResult Lock<TResult>(Func<TResult> action)
    {
        try
        {
            while (Interlocked.CompareExchange(ref _locked, 1, 0) != 0);
            
            return action();
        }
        finally
        {
            _locked = 0;
        }
    }

    public void Lock(Action action)
    {
        Lock<Nothing>(() =>
        {
            action();
            return default;
        });
    }
}