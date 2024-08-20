namespace BeeHive;

internal class TaskCancellationTokenSource : IDisposable
{
    private readonly CancellationTokenSource _taskCancellationTokenSource = new();
    private readonly CancellationTokenSource _poolAndTaskLinkedCancellationTokenSource;

    public TaskCancellationTokenSource(CancellationToken poolCancellationToken)
    {
        _taskCancellationTokenSource = new();
        _poolAndTaskLinkedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
            _taskCancellationTokenSource.Token,
            poolCancellationToken);
    }

    public CancellationToken Token => _poolAndTaskLinkedCancellationTokenSource.Token;

    public void Cancel() => _taskCancellationTokenSource.Cancel();
    
    public void Dispose()
    {
        _taskCancellationTokenSource.Dispose();
        _poolAndTaskLinkedCancellationTokenSource.Dispose();
    }
}
