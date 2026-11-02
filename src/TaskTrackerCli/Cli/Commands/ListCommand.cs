using TaskTrackerCli.Application;
using TaskTrackerCli.Domain;

namespace TaskTrackerCli.Cli.Commands;

public sealed class ListCommand(ITaskQueryService tasks, ITaskFormatter formatter, IOutput output) : ICommand
{
    public string Name => "list";

    public string Usage => $"list [{string.Join('|', TaskFilter.Known.Where(f => f != TaskFilter.All).Select(f => f.Name))}]";

    public string Description => "List tasks, optionally only those with a given status";

    public void Execute(IReadOnlyList<string> arguments)
    {
        if (arguments.Count > 1)
        {
            throw new UsageException($"Too many arguments. Usage: {Usage}");
        }

        var filter = TaskFilter.All;
        if (arguments.Count == 1 && !TaskFilter.TryParse(arguments[0], out filter))
        {
            throw new UsageException($"Unknown filter '{arguments[0]}'. Usage: {Usage}");
        }

        var matches = tasks.List(filter);
        output.WriteLine(matches.Count == 0 ? "No tasks found." : formatter.FormatTable(matches));
    }
}
