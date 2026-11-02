namespace TaskTrackerCli.Domain;

/// <summary>
/// Base type for every *expected* failure. The CLI shows these messages to the user
/// verbatim; anything else is treated as an unexpected bug.
/// </summary>
public abstract class TaskTrackerException : Exception
{
    protected TaskTrackerException(string message, Exception? inner = null) : base(message, inner) { }
}

public sealed class InvalidDescriptionException(string message) : TaskTrackerException(message);

public sealed class TaskNotFoundException(int id)
    : TaskTrackerException($"Task with ID {id} was not found.")
{
    public int TaskId { get; } = id;
}

/// <summary>The task store could not be read or written.</summary>
public sealed class StorageException(string message, Exception? inner = null)
    : TaskTrackerException(message, inner);
