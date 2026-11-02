using TaskTrackerCli.Application;
using TaskTrackerCli.Domain;

namespace TaskTrackerCli.Cli.Commands;

/// <summary>
/// Moves a task to a fixed status. One class serves <c>mark-todo</c>, <c>mark-in-progress</c>
/// and <c>mark-done</c>; the differences are plain data (see <see cref="InProgress"/> etc.).
/// </summary>
public sealed class MarkStatusCommand : ICommand
{
    private readonly ITaskCommandService _tasks;
    private readonly IOutput _output;
    private readonly TodoStatus _status;

    private MarkStatusCommand(ITaskCommandService tasks, IOutput output, TodoStatus status, string description)
    {
        _tasks = tasks;
        _output = output;
        _status = status;
        Name = $"mark-{status.ToName()}";
        Usage = $"{Name} <id>";
        Description = description;
    }

    public string Name { get; }
    public string Usage { get; }
    public string Description { get; }

    public static MarkStatusCommand Todo(ITaskCommandService tasks, IOutput output) =>
        new(tasks, output, TodoStatus.Todo, "Reset a task to todo");

    public static MarkStatusCommand InProgress(ITaskCommandService tasks, IOutput output) =>
        new(tasks, output, TodoStatus.InProgress, "Mark a task as in progress");

    public static MarkStatusCommand Done(ITaskCommandService tasks, IOutput output) =>
        new(tasks, output, TodoStatus.Done, "Mark a task as done");

    public void Execute(IReadOnlyList<string> arguments)
    {
        ArgumentParser.RequireExactly(arguments, 1, Usage);

        var id = ArgumentParser.ParseId(arguments[0]);
        var task = _tasks.SetStatus(id, _status);
        _output.WriteLine($"Task {task.Id} marked as {_status.ToName()}");
    }
}
