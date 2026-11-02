namespace TaskTrackerCli.Domain;

/// <summary>The lifecycle state of a task.</summary>
public enum TodoStatus
{
    Todo,
    InProgress,
    Done
}

/// <summary>
/// Single source of truth for the textual names of <see cref="TodoStatus"/> values.
/// The JSON file, the CLI arguments and the list output all share these names.
/// </summary>
public static class TodoStatusNames
{
    public static string ToName(this TodoStatus status) => status switch
    {
        TodoStatus.Todo => "todo",
        TodoStatus.InProgress => "in-progress",
        TodoStatus.Done => "done",
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unknown task status.")
    };

    public static bool TryParse(string? name, out TodoStatus status)
    {
        switch (name?.Trim().ToLowerInvariant())
        {
            case "todo": status = TodoStatus.Todo; return true;
            case "in-progress": status = TodoStatus.InProgress; return true;
            case "done": status = TodoStatus.Done; return true;
            default: status = default; return false;
        }
    }
}
