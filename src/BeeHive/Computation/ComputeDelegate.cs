namespace BeeHive;

public delegate ValueTask<TResult> Compute<TRequest, TResult>(TRequest request, CancellationToken cancellationToken);