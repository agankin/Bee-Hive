namespace BeeHive;

internal class ComputationTaskFactory<TRequest, TResult>
{
    private readonly Compute<TRequest, TResult> _compute;
    private readonly OnTaskCompleted<TRequest, TResult> _onCompleted;
    private readonly CancellationToken _poolCancellationToken;

    internal ComputationTaskFactory(
        Compute<TRequest, TResult> compute,
        OnTaskCompleted<TRequest, TResult> onCompleted,
        CancellationToken poolCancellationToken)
    {
        _compute = compute;
        _onCompleted = onCompleted;
        _poolCancellationToken = poolCancellationToken;
    }

    internal ComputationTask<TRequest, TResult> Create(TRequest request)
    {
        var taskCompletionSource = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        var task = taskCompletionSource.Task;
        var cancellationTokenSource = new CancellationTokenSource();

        HiveTask<TRequest, TResult> hiveTask = null!;
        var onCancelRequested = () => OnCancelRequested(hiveTask, cancellationTokenSource);
        hiveTask = new HiveTask<TRequest, TResult>(request, taskCompletionSource, onCancelRequested);
        
        var computation = () => 
        {
            using var aggregatedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationTokenSource.Token, _poolCancellationToken);
            
            Compute(hiveTask, aggregatedCancellationTokenSource.Token);
            cancellationTokenSource.Dispose();
        };
        
        return new(computation, hiveTask);
    }

    private void Compute(HiveTask<TRequest, TResult> task, CancellationToken cancellationToken)
    {
        if (!task.TrySetInProgress())
        {
            OnCancelled(task);
            return;
        }
        
        if (cancellationToken.IsCancellationRequested)
        {
            OnCancelled(task);
            return;
        }

        var request = task.Request;
        var awaiter = _compute(request, cancellationToken).GetAwaiter();
        
        awaiter.OnCompleted(() =>
        {
            try
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    OnCancelled(task);
                }
                else
                {
                    var result = Result<TRequest, TResult>.FromValue(request, awaiter.GetResult());
                    OnCompleted(task, result);
                }
            }
            catch (OperationCanceledException)
            {
                OnCancelled(task);
            }
            catch (Exception ex)
            {
                var errorResult = Result<TRequest, TResult>.FromError(request, ex);
                OnCompleted(task, errorResult);
            }
        });
    }

    private void OnCompleted(HiveTask<TRequest, TResult> task, Result<TRequest, TResult> result)
    {
        if (!task.TryComplete(result))
            return;
        
        _onCompleted(task, result);
    }

    private void OnCancelled(HiveTask<TRequest, TResult> task)
    {
        if (!task.TrySetPendingCancelled() && !task.TrySetInProgressCancelled())
            return;
        
        CompleteCancelled(task);
    }

    private void OnCancelRequested(HiveTask<TRequest, TResult> task, CancellationTokenSource cancellationTokenSource)
    {
        cancellationTokenSource.Cancel();
        
        if (!task.TrySetPendingCancelled())
            return;

        CompleteCancelled(task);
    }

    private void CompleteCancelled(HiveTask<TRequest, TResult> task)
    {
        var cancelledResult = Result<TRequest, TResult>.Cancelled(task.Request);
        
        task.Complete(cancelledResult);
        _onCompleted(task, cancelledResult);
    }
}