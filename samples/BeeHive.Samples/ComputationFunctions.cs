using System.Numerics;
using System.Text;

namespace BeeHive.Samples;

internal static class ComputationFunctions
{
    /// <summary>
    /// A sync function determining if an arbitrarily large number in string is prime.
    /// The implementation is inefficient but good as an example of a long running function.
    /// </summary>
    public static bool IsPrimeNumber(long number, CancellationToken cancellationToken)
    {
        Thread.Sleep(100);                                    // Simulates some work.
        cancellationToken.ThrowIfCancellationRequested();

        if (number < 0)
            throw new Exception("Number must be greater than or equal to zero.");

        if (number == 0 || number == 1)
            return false;

        if (number == 2)
            return true;

        if (number % 2 == 0)
            return false;

        var divisor = new BigInteger(3);
        var halfNumber = number / 2;
        
        while (divisor <= halfNumber)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (number % divisor == 0)
                return false;

            divisor += 2;
        }
        
        return true;
    }

    /// <summary>
    /// An async function computing the square root of an integer number.
    /// </summary>
    public static async Task<double> SqrtAsync(int value, CancellationToken cancellationToken)
    {
        // Simulates long work for 1 second.
        for (var i = 0; i < 10; i++)
        {
            await Task.Delay(100);
            cancellationToken.ThrowIfCancellationRequested();
        }

        if (value < 0)
            throw new Exception("Cannot calculate sqrt of negative value.");

        var result = Math.Sqrt(value);
        return result;
    }
}