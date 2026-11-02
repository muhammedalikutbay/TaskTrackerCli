using TaskTrackerCli.Application;

namespace TaskTrackerCli.Cli.Commands;

public sealed class DeleteCommand(ITaskCommandService tasks, IOutput output) : ICommand
{
    public string Name => "delete";
    public string Usage => "delete <id>";
    public string Description => "Delete a task";

    public void Execute(IReadOnlyList<string> arguments)
    {
        ArgumentParser.RequireExactly(arguments, 1, Usage);

        var id = ArgumentParser.ParseId(arguments[0]);
        tasks.Delete(id);
        output.WriteLine($"Task deleted successfully (ID: {id})");
    }
}
