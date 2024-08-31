namespace BeeHive;

/// <summary>
/// Contains possible states of a Hive task.
/// </summary>
public enum HiveTaskState
{
    /// <summary>
    /// The task has not been taken to work.
    /// </summary>
    Pending,

    /// <summary>
    /// The task is being performed now.
    /// </summary>
    InProgress,

    /// <summary>
    /// The task completed successfully.
    /// </summary>
    SuccessfullyCompleted,

    /// <summary>
    /// The task completed with an error.
    /// </summary>
    Error,

    /// <summary>
    /// The task was cancelled.
    /// </summary>
    Cancelled
}