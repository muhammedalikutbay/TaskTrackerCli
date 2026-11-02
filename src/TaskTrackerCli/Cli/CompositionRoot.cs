using TaskTrackerCli.Application;
using TaskTrackerCli.Cli.Commands;
using TaskTrackerCli.Infrastructure;

namespace TaskTrackerCli.Cli;

/// <summary>
/// The only place that knows about concrete implementations. Everything else depends on
/// abstractions and receives its collaborators through the constructor.
/// </summary>
public static class CompositionRoot
{
    public const string ProgramName = "task-cli";
    public const string DefaultFileName = "tasks.json";

    public static CliApplication Create(
        string tasksFilePath,
        IOutput output,
        IClock? clock = null,
        ITaskFormatter? formatter = null)
    {
        ITaskRepository repository = new JsonFileTaskRepository(tasksFilePath);
        var service = new TaskService(repository, clock ?? new SystemClock());
        formatter ??= new TaskTableFormatter();

        ICommand[] commands =
        [
            new AddCommand(service, output),
            new UpdateCommand(service, output),
            new DeleteCommand(service, output),
            MarkStatusCommand.InProgress(service, output),
            MarkStatusCommand.Done(service, output),
            MarkStatusCommand.Todo(service, output),
            new ListCommand(service, formatter, output)
        ];

        var registry = new CommandRegistry(commands);
        return new CliApplication(registry, new HelpPrinter(registry, ProgramName), output);
    }
}
