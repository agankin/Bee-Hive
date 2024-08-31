namespace BeeHive;

/// <summary>
/// Contains possible states of a computation result.
/// </summary>
public enum ResultState : byte
{
    /// <summary>
    /// Computation completed successfully.
    /// </summary>
    Success = 1,

    /// <summary>
    /// Computation failed.
    /// </summary>
    Error,

    /// <summary>
    /// Computation cancelled.
    /// </summary>
    Cancelled
}