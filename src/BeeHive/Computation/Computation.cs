namespace BeeHive;

public class Computation<TRequest, TResult>
{
    private readonly Action<Computation<TRequest, TResult>> _compute;
    
    private volatile ComputationStatus _status = ComputationStatus.Pending;
    private volatile Result<TRequest, TResult> _result = null!;

    public Computation(TRequest request, Action<Computation<TRequest, TResult>> compute, Task<Result<TRequest, TResult>> task)
    {
        Request = request;
        _compute = compute;
        Task = task;
    }

    public TRequest Request { get; }

    public Task<Result<TRequest, TResult>> Task { get; }

    public ComputationStatus Status
    {
        get => _status;
        private set => _status = value;
    }

    public Result<TRequest, TResult> Result
    {
        get => _result;
        private set => _result = value;
    }

    public void Compute()
    {
        Status = ComputationStatus.InProgress;
        _compute(this);
    }

    public void SetCompleted(Result<TRequest, TResult> result)
    {
        if (_status == ComputationStatus.Completed)
            return;

        Result = result;
        Status = ComputationStatus.Completed;
    }

    public void Cancel() => SetCompleted(Result<TRequest, TResult>.Cancelled(Request));
}