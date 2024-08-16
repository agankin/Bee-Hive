using System.Runtime.CompilerServices;

namespace BeeHive;

internal class ComputationTaskFactory<TRequest, TResult>
{
    private readonly Compute<TRequest, TResult> _compute;
    private readonly OnTaskCompleted<TRequest, TResult> _onCompleted;
    private readonly OnTaskCancelled<TRequest, TResult> _onCancelled;
    private readonly CancellationToken _poolCancellationToken;

    internal ComputationTaskFactory(
        Compute<TRequest, TResult> compute,
        OnTaskCompleted<TRequest, TResult> onCompleted,
        OnTaskCancelled<TRequest, TResult> onCancelled,
        CancellationToken poolCancellationToken)
    {
        _compute = compute;
        _onCompleted = onCompleted;
        _onCancelled = onCancelled;
        _poolCancellationToken = poolCancellationToken;
    }

    internal HiveTask<TRequest, TResult> Create(TRequest request)
    {
        var taskCompletionSource = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        var task = taskCompletionSource.Task;
        var cancellationTokenSource = new CancellationTokenSource();

        HiveTask<TRequest, TResult> hiveTask = null!;
        
        var onTaskCancelRequested = () => OnTaskCancelRequested(hiveTask, cancellationTokenSource);
        var computation = () => 
        {
            var aggregatedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationTokenSource.Token, _poolCancellationToken);
            
            Compute(hiveTask, aggregatedCancellationTokenSource.Token);
            cancellationTokenSource.Dispose();
        };
        
        hiveTask = new HiveTask<TRequest, TResult>(request, computation, taskCompletionSource, onTaskCancelRequested);
        
        return hiveTask;
    }

    private void Compute(HiveTask<TRequest, TResult> task, CancellationToken cancellationToken)
    {
        if (!task.TrySetInProgress())
            return;
        
        if (cancellationToken.IsCancellationRequested)
        {
            OnCancelled(task);
            return;
        }

        var awaiter = ComputeCore(task.Request, cancellationToken).GetAwaiter();
        if (awaiter.IsCompleted)
        {
            OnComputeCompleted(task, cancellationToken, awaiter);
        }
        else
        {
            awaiter.OnCompleted(() => OnComputeCompleted(task, cancellationToken, awaiter));
        }
    }

    private ValueTask<TResult> ComputeCore(TRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return _compute(request, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            var task = Task.FromCanceled<TResult>(cancellationToken);
            return new ValueTask<TResult>(task);
        }
        catch (Exception ex)
        {
            var task = Task.FromException<TResult>(ex);
            return new ValueTask<TResult>(task);
        }
    }

    private void OnComputeCompleted(HiveTask<TRequest, TResult> task, CancellationToken cancellationToken, ValueTaskAwaiter<TResult> awaiter)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            OnCancelled(task);
            return;
        }

        var request = task.Request;
        
        try
        {
            var resultValue = awaiter.GetResult();
            var result = Result<TRequest, TResult>.FromValue(request, resultValue);
            
            OnCompleted(task, result);
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
    }

    private void OnCompleted(HiveTask<TRequest, TResult> task, Result<TRequest, TResult> result)
    {        
        task.Complete(result);
        _onCompleted(task, result);
    }

    private void OnCancelled(HiveTask<TRequest, TResult> task)
    {
        var cancelledResult = Result<TRequest, TResult>.Cancelled(task.Request);
        OnCompleted(task, cancelledResult);
    }

    private void OnTaskCancelRequested(HiveTask<TRequest, TResult> task, CancellationTokenSource cancellationTokenSource)
    {
        cancellationTokenSource.Cancel();
        
        if (!task.TrySetPendingCancelled())
            return;

        var cancelledResult = Result<TRequest, TResult>.Cancelled(task.Request);
        task.Complete(cancelledResult);
        _onCancelled(task);
    }
}