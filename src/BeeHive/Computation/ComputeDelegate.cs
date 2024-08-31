namespace BeeHive;

/// <summary>
/// A computation delegate.
/// </summary>
/// <param name="request">A request.</param>
/// <param name="cancellationToken">A cancellation token passed for cancelling computation.</param>
/// <typeparam name="TRequest">The request type of the computation.</typeparam>
/// <typeparam name="TResult">The result type of the computation.</typeparam>
/// <returns>A task representing computation result.</returns>
public delegate ValueTask<TResult> Compute<TRequest, TResult>(TRequest request, CancellationToken cancellationToken);