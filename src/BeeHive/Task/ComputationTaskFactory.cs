namespace BeeHive;

internal class ComputationFactory<TRequest, TResult>
{
    private readonly Compute<TRequest, TResult> _compute;
    private readonly Action<Computation<TRequest, TResult>> _onCompleted;

    internal ComputationFactory(Compute<TRequest, TResult> compute, Action<Computation<TRequest, TResult>> onCompleted)
    {
        _compute = compute;
        _onCompleted = onCompleted;
    }

    internal ComputationTask<TRequest, TResult> Create(TRequest request)
    {
        var taskCompletionSource = new TaskCompletionSource<Result<TRequest, TResult>>(TaskCreationOptions.RunContinuationsAsynchronously);
        var task = taskCompletionSource.Task;
        var cancellationTokenSource = new CancellationTokenSource();
        
        var computation = new Computation<TRequest, TResult>(
            request,
            self => Compute(self, cancellationTokenSource.Token, taskCompletionSource),
            task);
        var hiveTask = new HiveTask<TRequest, TResult>(request, task, cancellationTokenSource);
        
        return new(computation, hiveTask);
    }

    private void Compute(
        Computation<TRequest, TResult> computation,
        CancellationToken cancellationToken,
        TaskCompletionSource<Result<TRequest, TResult>> taskCompletionSource)
    {
        var request = computation.Request;
        if (cancellationToken.IsCancellationRequested)
        {
            var result = Result<TRequest, TResult>.Cancelled(request);
            OnCompleted(computation, result, taskCompletionSource);
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
                OnCompleted(computation, result, taskCompletionSource);
            }
            catch (OperationCanceledException)
            {
                var cancelledResult = Result<TRequest, TResult>.Cancelled(request);
                OnCompleted(computation, cancelledResult, taskCompletionSource);
            }
            catch (Exception ex)
            {
                var errorResult = Result<TRequest, TResult>.FromError(request, ex);
                OnCompleted(computation, errorResult, taskCompletionSource);
            }
        });
    }

    private void OnCompleted(
        Computation<TRequest, TResult> computation,
        Result<TRequest, TResult> result,
        TaskCompletionSource<Result<TRequest, TResult>> taskCompletionSource)
    {
        computation.SetCompleted(result);  

        _onCompleted(computation);
        taskCompletionSource.SetResult(result);
    }
}