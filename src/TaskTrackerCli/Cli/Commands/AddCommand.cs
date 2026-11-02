using TaskTrackerCli.Application;

namespace TaskTrackerCli.Cli.Commands;

public sealed class AddCommand(ITaskCommandService tasks, IOutput output) : ICommand
{
    public string Name => "add";
    public string Usage => "add \"<description>\"";
    public string Description => "Add a new task";

    public void Execute(IReadOnlyList<string> arguments)
    {
        if (arguments.Count == 0)
        {
            throw new UsageException($"Missing task description. Usage: {Usage}");
        }

        var task = tasks.Add(ArgumentParser.JoinText(arguments));
        output.WriteLine($"Task added successfully (ID: {task.Id})");
    }
}
