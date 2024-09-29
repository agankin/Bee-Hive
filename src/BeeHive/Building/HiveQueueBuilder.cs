namespace BeeHive;

/// <summary>
/// Contains extension methods for creating instances of <see cref="HiveQueue{TRequest, TResult}"/>.
/// </summary>
public static class HiveQueueBuilder
{
    /// <summary>
    /// Creates a Hive Queue for the Hive.
    /// </summary>
    /// <param name="hive">The Hive.</param>
    /// <param name="computationFunc">A computation delegate.</param>
    /// <typeparam name="TRequest">The type of computation request.</typeparam>
    /// <typeparam name="TResult">The type of computation result.</typeparam>
    /// <returns>A new instance of <see cref="HiveQueue{TRequest, TResult}"/>.</returns>
    public static HiveQueue<TRequest, TResult> GetQueueFor<TRequest, TResult>(this Hive hive, Func<TRequest, TResult> computationFunc)
    {
        var computeAsync = computationFunc.ToComputeDelegate();
        return hive.GetQueueFor(computeAsync);
    }

    /// <summary>
    /// Creates a Hive Queue for the Hive.
    /// </summary>
    /// <param name="hive">The Hive.</param>
    /// <param name="computationFunc">A computation delegate.</param>
    /// <typeparam name="TRequest">The type of computation request.</typeparam>
    /// <typeparam name="TResult">The type of computation result.</typeparam>
    /// <returns>A new instance of <see cref="HiveQueue{TRequest, TResult}"/>.</returns>
    public static HiveQueue<TRequest, TResult> GetQueueFor<TRequest, TResult>(this Hive hive, Func<TRequest, CancellationToken, TResult> computationFunc)
    {
        var computeAsync = computationFunc.ToComputeDelegate();
        return hive.GetQueueFor(computeAsync);
    }

    /// <summary>
    /// Creates a Hive Queue for the Hive.
    /// </summary>
    /// <param name="hive">The Hive.</param>
    /// <param name="computationFunc">A computation delegate.</param>
    /// <typeparam name="TRequest">The type of computation request.</typeparam>
    /// <typeparam name="TResult">The type of computation result.</typeparam>
    /// <returns>A new instance of <see cref="HiveQueue{TRequest, TResult}"/>.</returns>
    public static HiveQueue<TRequest, TResult> GetQueueFor<TRequest, TResult>(this Hive hive, Func<TRequest, Task<TResult>> computationFunc)
    {
        var computeAsync = computationFunc.ToComputeDelegate();
        return hive.GetQueueFor(computeAsync);
    }

    /// <summary>
    /// Creates a Hive Queue for the Hive.
    /// </summary>
    /// <param name="hive">The Hive.</param>
    /// <param name="computationFunc">A computation delegate.</param>
    /// <typeparam name="TRequest">The type of computation request.</typeparam>
    /// <typeparam name="TResult">The type of computation result.</typeparam>
    /// <returns>A new instance of <see cref="HiveQueue{TRequest, TResult}"/>.</returns>
    public static HiveQueue<TRequest, TResult> GetQueueFor<TRequest, TResult>(this Hive hive, Func<TRequest, CancellationToken, Task<TResult>> computationFunc)
    {
        var computeAsync = computationFunc.ToComputeDelegate();
        return hive.GetQueueFor(computeAsync);
    }

    /// <summary>
    /// Creates a Hive Queue for the Hive.
    /// </summary>
    /// <param name="hive">The Hive.</param>
    /// <param name="computationFunc">A computation delegate.</param>
    /// <typeparam name="TRequest">The type of computation request.</typeparam>
    /// <typeparam name="TResult">The type of computation result.</typeparam>
    /// <returns>A new instance of <see cref="HiveQueue{TRequest, TResult}"/>.</returns>
    public static HiveQueue<TRequest, TResult> GetQueueFor<TRequest, TResult>(this Hive hive, Func<TRequest, ValueTask<TResult>> computationFunc)
    {
        var computeAsync = computationFunc.ToComputeDelegate();
        return hive.GetQueueFor(computeAsync);
    }

    /// <summary>
    /// Creates a Hive Queue for the Hive.
    /// </summary>
    /// <param name="hive">The Hive.</param>
    /// <param name="computationFunc">A computation delegate.</param>
    /// <typeparam name="TRequest">The type of computation request.</typeparam>
    /// <typeparam name="TResult">The type of computation result.</typeparam>
    /// <returns>A new instance of <see cref="HiveQueue{TRequest, TResult}"/>.</returns>
    public static HiveQueue<TRequest, TResult> GetQueueFor<TRequest, TResult>(this Hive hive, Func<TRequest, CancellationToken, ValueTask<TResult>> computationFunc)
    {
        var computeAsync = computationFunc.ToComputeDelegate();
        return hive.GetQueueFor(computeAsync);
    }
}