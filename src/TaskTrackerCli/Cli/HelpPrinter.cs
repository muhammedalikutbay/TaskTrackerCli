using System.Text;

namespace TaskTrackerCli.Cli;

/// <summary>Builds the help text from whatever commands are registered, so it can never go stale.</summary>
public sealed class HelpPrinter(CommandRegistry registry, string programName)
{
    public string Build()
    {
        var commands = registry.All.ToList();
        var width = commands.Max(c => c.Usage.Length);

        var builder = new StringBuilder();
        builder.AppendLine($"Usage: {programName} <command> [arguments]");
        builder.AppendLine();
        builder.AppendLine("Commands:");
        foreach (var command in commands)
        {
            builder.AppendLine($"  {command.Usage.PadRight(width)}  {command.Description}");
        }

        builder.AppendLine($"  {"help".PadRight(width)}  Show this help");
        builder.AppendLine();
        builder.Append("Tasks are stored in tasks.json in the current directory.");
        return builder.ToString();
    }
}
