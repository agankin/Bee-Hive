namespace BeeHive;

public delegate ValueTask<TResult> AsyncFunc<TRequest, TResult>(TRequest request);