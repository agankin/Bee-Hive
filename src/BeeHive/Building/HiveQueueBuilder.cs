namespace BeeHive;

/// <summary>
/// Contains extension methods for creating Hive queues.
/// </summary>
public static class HiveQueueBuilder
{
    /// <summary>
    /// Creates a Hive queue.
    /// </summary>
    /// <param name="hive">An instance of Hive.</param>
    /// <param name="computationFunc">A computation delegate.</param>
    /// <typeparam name="TRequest">The request type of the computation.</typeparam>
    /// <typeparam name="TResult">The result type of the computation.</typeparam>
    /// <returns>A new instance of Hive queue.</returns>
    public static HiveQueue<TRequest, TResult> GetQueueFor<TRequest, TResult>(this Hive hive, Func<TRequest, TResult> computationFunc)
    {
        var computeAsync = computationFunc.ToComputeDelegate();
        return hive.GetQueueFor(computeAsync);
    }

    /// <summary>
    /// Creates a Hive queue.
    /// </summary>
    /// <param name="hive">An instance of Hive.</param>
    /// <param name="computationFunc">A computation delegate.</param>
    /// <typeparam name="TRequest">The request type of the computation.</typeparam>
    /// <typeparam name="TResult">The result type of the computation.</typeparam>
    /// <returns>A new instance of Hive queue.</returns>
    public static HiveQueue<TRequest, TResult> GetQueueFor<TRequest, TResult>(this Hive hive, Func<TRequest, CancellationToken, TResult> computationFunc)
    {
        var computeAsync = computationFunc.ToComputeDelegate();
        return hive.GetQueueFor(computeAsync);
    }

    /// <summary>
    /// Creates a Hive queue.
    /// </summary>
    /// <param name="hive">An instance of Hive.</param>
    /// <param name="computationFunc">A computation delegate.</param>
    /// <typeparam name="TRequest">The request type of the computation.</typeparam>
    /// <typeparam name="TResult">The result type of the computation.</typeparam>
    /// <returns>A new instance of Hive queue.</returns>
    public static HiveQueue<TRequest, TResult> GetQueueFor<TRequest, TResult>(this Hive hive, Func<TRequest, Task<TResult>> computationFunc)
    {
        var computeAsync = computationFunc.ToComputeDelegate();
        return hive.GetQueueFor(computeAsync);
    }

    /// <summary>
    /// Creates a Hive queue.
    /// </summary>
    /// <param name="hive">An instance of Hive.</param>
    /// <param name="computationFunc">A computation delegate.</param>
    /// <typeparam name="TRequest">The request type of the computation.</typeparam>
    /// <typeparam name="TResult">The result type of the computation.</typeparam>
    /// <returns>A new instance of Hive queue.</returns>
    public static HiveQueue<TRequest, TResult> GetQueueFor<TRequest, TResult>(this Hive hive, Func<TRequest, CancellationToken, Task<TResult>> computationFunc)
    {
        var computeAsync = computationFunc.ToComputeDelegate();
        return hive.GetQueueFor(computeAsync);
    }

    /// <summary>
    /// Creates a Hive queue.
    /// </summary>
    /// <param name="hive">An instance of Hive.</param>
    /// <param name="computationFunc">A computation delegate.</param>
    /// <typeparam name="TRequest">The request type of the computation.</typeparam>
    /// <typeparam name="TResult">The result type of the computation.</typeparam>
    /// <returns>A new instance of Hive queue.</returns>
    public static HiveQueue<TRequest, TResult> GetQueueFor<TRequest, TResult>(this Hive hive, Func<TRequest, ValueTask<TResult>> computationFunc)
    {
        var computeAsync = computationFunc.ToComputeDelegate();
        return hive.GetQueueFor(computeAsync);
    }

    /// <summary>
    /// Creates a Hive queue.
    /// </summary>
    /// <param name="hive">An instance of Hive.</param>
    /// <param name="computationFunc">A computation delegate.</param>
    /// <typeparam name="TRequest">The request type of the computation.</typeparam>
    /// <typeparam name="TResult">The result type of the computation.</typeparam>
    /// <returns>A new instance of Hive queue.</returns>
    public static HiveQueue<TRequest, TResult> GetQueueFor<TRequest, TResult>(this Hive hive, Func<TRequest, CancellationToken, ValueTask<TResult>> computationFunc)
    {
        var computeAsync = computationFunc.ToComputeDelegate();
        return hive.GetQueueFor(computeAsync);
    }
}