using TaskTrackerCli.Domain;

namespace TaskTrackerCli.Cli;

/// <summary>
/// Dispatches a command line to the matching <see cref="ICommand"/> and converts every
/// outcome into an exit code. It is the only place that decides how errors are reported.
/// </summary>
public sealed class CliApplication(CommandRegistry registry, HelpPrinter help, IOutput output)
{
    private static readonly string[] HelpAliases = ["help", "--help", "-h", "/?"];

    public int Run(string[] args)
    {
        if (args.Length == 0)
        {
            output.WriteError(help.Build());
            return ExitCodes.Usage;
        }

        var name = args[0];
        if (HelpAliases.Contains(name, StringComparer.OrdinalIgnoreCase))
        {
            output.WriteLine(help.Build());
            return ExitCodes.Success;
        }

        if (!registry.TryGet(name, out var command))
        {
            output.WriteError($"Unknown command '{name}'. Run 'help' to see the available commands.");
            return ExitCodes.Usage;
        }

        try
        {
            command.Execute(args[1..]);
            return ExitCodes.Success;
        }
        catch (UsageException ex)
        {
            output.WriteError($"Error: {ex.Message}");
            return ExitCodes.Usage;
        }
        catch (TaskTrackerException ex)
        {
            output.WriteError($"Error: {ex.Message}");
            return ExitCodes.Failure;
        }
        catch (Exception ex)
        {
            output.WriteError($"Unexpected error: {ex.Message}");
            return ExitCodes.Failure;
        }
    }
}
