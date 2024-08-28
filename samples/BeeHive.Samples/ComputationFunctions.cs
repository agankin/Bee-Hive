namespace BeeHive.Samples;

internal static class ComputationFunctions
{
    /// <summary>
    /// A sync function computing twice of integers.
    /// The second param "cancellationToken" can be ommited if cancellation isn't supported.
    /// </summary>
    public static int Twice(int value, CancellationToken cancellationToken)
    {
        // Simulates long work for 1 second.
        for (var i = 0; i < 10; i++)
        {
            Thread.Sleep(100);
            cancellationToken.ThrowIfCancellationRequested();
        }
        
        return value * 2;
    }

    /// <summary>
    /// An async function computing integer square root.
    /// The second param "cancellationToken" can be ommited if cancellation isn't supported. 
    /// </summary>
    public static async Task<int> SqrtAsync(int value, CancellationToken cancellationToken)
    {
        // Simulates long work for 1 second.
        for (var i = 0; i < 10; i++)
        {
            await Task.Delay(100);
            cancellationToken.ThrowIfCancellationRequested();
        }

        if (value < 0)
            throw new Exception("Cannot calculate sqrt of negative value.");

        var sqrtDouble = Math.Sqrt(value);
        var sqrtInt = (int)Math.Round(sqrtDouble);
        
        return sqrtInt;
    }
}