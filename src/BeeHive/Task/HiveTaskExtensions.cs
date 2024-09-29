namespace BeeHive;

/// <summary>
/// Contains extension methods for <see cref="HiveTask{TRequest, TResult}"/>.
/// </summary>
public static class HiveTaskExtensions
{
    /// <summary>
    /// Awaits the Hive Task suppressing exceptions and returns Result that can be in 3 states: a value, an error or cancelled.
    /// </summary>
    /// <param name="hiveTask">The Hive Task.</param>
    /// <typeparam name="TRequest">The type of computation request.</typeparam>
    /// <typeparam name="TResult">The type of computation result.</typeparam>
    /// <returns>A Task representing computation result.</returns>
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