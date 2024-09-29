namespace BeeHive;

/// <summary>
/// A computation delegate.
/// </summary>
/// <param name="request">A request.</param>
/// <param name="cancellationToken">A cancellation token passed for cancelling computation.</param>
/// <typeparam name="TRequest">The type of computation request.</typeparam>
/// <typeparam name="TResult">The type of computation result.</typeparam>
/// <returns>A Task representing computation result.</returns>
public delegate ValueTask<TResult> Compute<TRequest, TResult>(TRequest request, CancellationToken cancellationToken);