namespace BeeHive;

/// <summary>
/// Contains extension methods for <see cref="HiveTask{TRequest, TResult}"/>.
/// </summary>
public static class HiveTaskExtensions
{
    /// <summary>
    /// Awaits for Result without throwing exceptions and returns Result that can be in 3 states: a value, an error or cancelled.
    /// </summary>
    /// <param name="hiveTask">The Hive Task.</param>
    /// <typeparam name="TRequest">The request type of the computation.</typeparam>
    /// <typeparam name="TResult">The result type of the computation.</typeparam>
    /// <returns>A task representing computation result.</returns>
    public static async Task<Result<TRequest, TResult>> AsyncResult<TRequest, TResult>(this HiveTask<TRequest, TResult> hiveTask)
    {
        try
        {
            await hiveTask;
        }
        catch {}

        return hiveTask.Result.NotNull("hiveTask.Result");
    }
}