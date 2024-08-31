namespace BeeHive;

/// <summary>
/// Represents a computation result.
/// </summary>
/// <param name="Request">Contains a request the computation was performed for.</param>
/// <param name="State">Contains a state of the performed computation.</param>
/// <param name="Value">Contains a result value of the computation if it completed with success.</param>
/// <param name="Error">Contains an error if the computation failed.</param>
/// <typeparam name="TRequest">The request type of the computation.</typeparam>
/// <typeparam name="TResult">The result type of the computation.</typeparam>
public record Result<TRequest, TResult>(
    TRequest Request,
    ResultState State,
    TResult? Value,
    Exception? Error
)
{
    internal static Result<TRequest, TResult> FromValue(TRequest request, TResult value) =>
        new(request, ResultState.Success, value, Error: default);
    
    internal static Result<TRequest, TResult> FromError(TRequest request, Exception error) =>
        new(request, ResultState.Error, Value: default, Error: error.ArgNotNull(nameof(error)));

    internal static Result<TRequest, TResult> Cancelled(TRequest request) =>
        new(request, ResultState.Cancelled, Value: default, Error: default);
}