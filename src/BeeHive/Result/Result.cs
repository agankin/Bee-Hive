namespace BeeHive;

public record Result<TRequest, TResult>(
    TRequest Request,
    ResultState State,
    TResult? Value,
    Exception? Error
)
{
    public static Result<TRequest, TResult> FromValue(TRequest request, TResult value) =>
        new(request, ResultState.Success, value, Error: default);
    
    public static Result<TRequest, TResult> FromError(TRequest request, Exception error) =>
        new(request, ResultState.Error, Value: default, Error: error.ArgNotNull(nameof(error)));

    public static Result<TRequest, TResult> Cancelled(TRequest request) =>
        new(request, ResultState.Cancelled, Value: default, Error: default);
}