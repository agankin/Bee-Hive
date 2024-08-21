namespace BeeHive;

public class LiteSemaphore
{
    private readonly object _syncObject = new object();
    private volatile int _counter;

    public bool Wait(int waitMilliseconds, CancellationToken cancellationToken)
    {
        using var timeoutTokenSource = new CancellationTokenSource(waitMilliseconds);
        using var waitCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutTokenSource.Token);

        var waitCancellationToken = waitCancellationTokenSource.Token;
        return Wait(waitCancellationToken);
    }
    
    public bool Wait(CancellationToken cancellationToken)
    {
        using var _ = cancellationToken.Register(OnCancellation);
        
        lock (_syncObject)
        {
            if (_counter > 0)
            {
                _counter--;
                return true;
            }

            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                    return false;

                Monitor.Wait(_syncObject);

                if (_counter > 0)
                {
                    _counter--;
                    return true;
                }
            }
        }
    }

    public void Release()
    {
        lock (_syncObject)
        {
            _counter++;
            Monitor.Pulse(_syncObject);
        }
    }

    private void OnCancellation()
    {
        lock (_syncObject)
            Monitor.PulseAll(_syncObject);
    }
}