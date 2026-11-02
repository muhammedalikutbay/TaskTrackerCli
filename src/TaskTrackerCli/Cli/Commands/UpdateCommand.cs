using TaskTrackerCli.Application;

namespace TaskTrackerCli.Cli.Commands;

public sealed class UpdateCommand(ITaskCommandService tasks, IOutput output) : ICommand
{
    public string Name => "update";
    public string Usage => "update <id> \"<description>\"";
    public string Description => "Change the description of a task";

    public void Execute(IReadOnlyList<string> arguments)
    {
        if (arguments.Count < 2)
        {
            throw new UsageException($"Expected a task ID and a new description. Usage: {Usage}");
        }

        var id = ArgumentParser.ParseId(arguments[0]);
        var task = tasks.UpdateDescription(id, ArgumentParser.JoinText(arguments.Skip(1)));
        output.WriteLine($"Task updated successfully (ID: {task.Id})");
    }
}
