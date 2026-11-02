namespace TaskTrackerCli.Domain;

/// <summary>
/// A named selection of tasks used by <c>list</c>. New filters are added by declaring
/// another instance; no existing code has to change (open/closed).
/// </summary>
public sealed class TaskFilter
{
    private readonly Func<TaskItem, bool> _predicate;

    private TaskFilter(string name, string description, Func<TaskItem, bool> predicate)
    {
        Name = name;
        Description = description;
        _predicate = predicate;
    }

    public string Name { get; }

    public string Description { get; }

    public bool Matches(TaskItem task) => _predicate(task);

    public static TaskFilter All { get; } =
        new("all", "every task (default)", _ => true);

    public static TaskFilter Todo { get; } =
        new("todo", "tasks that have not been started", t => t.Status == TodoStatus.Todo);

    public static TaskFilter InProgress { get; } =
        new("in-progress", "tasks currently being worked on", t => t.Status == TodoStatus.InProgress);

    public static TaskFilter Done { get; } =
        new("done", "completed tasks", t => t.Status == TodoStatus.Done);

    public static TaskFilter NotDone { get; } =
        new("not-done", "tasks that are not done yet (todo + in-progress)", t => t.Status != TodoStatus.Done);

    public static IReadOnlyList<TaskFilter> Known { get; } = [All, Todo, InProgress, Done, NotDone];

    public static bool TryParse(string? name, out TaskFilter filter)
    {
        var normalized = name?.Trim().ToLowerInvariant();
        var match = Known.FirstOrDefault(f => f.Name == normalized);
        filter = match ?? All;
        return match is not null;
    }
}
