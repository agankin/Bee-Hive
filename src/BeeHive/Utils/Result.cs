namespace BeeHive;

public record Result<TValue>(
    ResultState State,
    TValue? Value,
    Exception? Error
)
{
    public static Result<TValue> FromValue(TValue? value) =>
        new(State: ResultState.Success, value, Error: default);
    
    public static Result<TValue> FromError(Exception error) =>
        new(State: ResultState.Error, Value: default, Error: error.ArgNotNull(nameof(error)));

    public static Result<TValue> Cancelled() =>
        new(State: ResultState.Cancelled, Value: default, Error: default);
}