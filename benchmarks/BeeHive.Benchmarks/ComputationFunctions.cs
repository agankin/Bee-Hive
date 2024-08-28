namespace BeeHive.Benchmarks;

internal static class ComputationFunctions
{
    public static long TenfoldSync(long value)
    {
        var result = 0L;
        
        result += value;
        result += value;
        result += value;
        result += value;
        result += value;
        result += value;
        result += value;
        result += value;
        result += value;
        result += value;

        return result;
    }

    public static async Task<long> TenfoldAsync(long value)
    {
        var result = 0L;
        
        result += value;
        await Task.Yield();

        result += value;
        await Task.Yield();

        result += value;
        await Task.Yield();
        
        result += value;
        await Task.Yield();
        
        result += value;
        await Task.Yield();

        result += value;
        await Task.Yield();

        result += value;
        await Task.Yield();

        result += value;
        await Task.Yield();

        result += value;
        await Task.Yield();

        result += value;
        await Task.Yield();

        return result;
    }
}