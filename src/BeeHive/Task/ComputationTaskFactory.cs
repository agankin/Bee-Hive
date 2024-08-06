namespace BeeHive;

internal class ComputationTaskFactory<TRequest, TResult>
{
    private readonly Compute<TRequest, TResult> _compute;
    private readonly OnTaskCompleted<TRequest, TResult> _onCompleted;

    internal ComputationTaskFactory(Compute<TRequest, TResult> compute, OnTaskCompleted<TRequest, TResult> onCompleted)
    {
        _compute = compute;
        _onCompleted = onCompleted;
    }

    internal ComputationTask<TRequest, TResult> Create(TRequest request)
    {
        var taskCompletionSource = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        var task = taskCompletionSource.Task;
        var cancellationTokenSource = new CancellationTokenSource();
        
        var hiveTask = new HiveTask<TRequest, TResult>(request, taskCompletionSource, cancellationTokenSource);
        var computation = () => Compute(hiveTask, cancellationTokenSource.Token);      
        
        return new(computation, hiveTask);
    }

    private void Compute(HiveTask<TRequest, TResult> task, CancellationToken cancellationToken)
    {
        var request = task.Request;

        if (cancellationToken.IsCancellationRequested)
        {
            var result = Result<TRequest, TResult>.Cancelled(request);
            OnCompleted(task, result);
            return;
        }

        var awaiter = _compute(request, cancellationToken).GetAwaiter();
        awaiter.OnCompleted(() =>
        {
            try
            {
                var result = cancellationToken.IsCancellationRequested
                    ? Result<TRequest, TResult>.Cancelled(request)
                    : Result<TRequest, TResult>.FromValue(request, awaiter.GetResult());
                OnCompleted(task, result);
            }
            catch (OperationCanceledException)
            {
                var cancelledResult = Result<TRequest, TResult>.Cancelled(request);
                OnCompleted(task, cancelledResult);
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
        task.Complete(result);
        _onCompleted(task, result);
    }
}