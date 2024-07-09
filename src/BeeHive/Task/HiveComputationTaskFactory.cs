namespace BeeHive;

internal class HiveComputationFactory<TRequest, TResult>
{
    private readonly Compute<TRequest, TResult> _compute;
    private readonly Action<Result<TResult>> _onCompleted;

    internal HiveComputationFactory(Compute<TRequest, TResult> compute, Action<Result<TResult>> onCompleted)
    {
        _compute = compute;
        _onCompleted = onCompleted;
    }

    internal HiveComputationTask<TResult> Create(TRequest request)
    {
        var taskCompletionSource = new TaskCompletionSource<Result<TResult>>(TaskCreationOptions.RunContinuationsAsynchronously);
        var cancellationTokenSource = new CancellationTokenSource();
        
        void OnCompleted(Result<TResult> result)
        {
            _onCompleted(result);
            taskCompletionSource.SetResult(result);
        }

        void Compute()
        {
            var cancellationToken = cancellationTokenSource.Token;
            var awaiter = _compute(request, cancellationToken).GetAwaiter();

            awaiter.OnCompleted(() =>
            {
                try
                {
                    var result = cancellationToken.IsCancellationRequested
                        ? Result<TResult>.Cancelled()
                        : Result<TResult>.FromValue(awaiter.GetResult());
                    OnCompleted(result);
                }
                catch (OperationCanceledException)
                {
                    var cancelledResult = Result<TResult>.Cancelled();
                    OnCompleted(cancelledResult);
                }
                catch (Exception ex)
                {
                    var errorResult = Result<TResult>.FromError(ex);
                    OnCompleted(errorResult);
                }
            });
        }

        var task = new HiveTask<TResult>(taskCompletionSource.Task, cancellationTokenSource);
        var computationTask = new HiveComputationTask<TResult>(Compute, task);
        
        return computationTask;
    }
}