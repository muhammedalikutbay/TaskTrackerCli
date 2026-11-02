namespace TaskTrackerCli.Cli;

/// <summary>
/// One CLI verb (<c>add</c>, <c>list</c>, ...). Adding a feature means adding a class that
/// implements this interface and registering it in the composition root; the dispatcher
/// never changes (open/closed).
/// </summary>
public interface ICommand
{
    /// <summary>The word the user types, e.g. <c>mark-done</c>.</summary>
    string Name { get; }

    /// <summary>Argument synopsis for help and error messages, e.g. <c>mark-done &lt;id&gt;</c>.</summary>
    string Usage { get; }

    /// <summary>One-line explanation shown in the help text.</summary>
    string Description { get; }

    /// <summary>
    /// Runs the command. <paramref name="arguments"/> excludes the command name itself.
    /// Expected failures are signalled by throwing a <c>TaskTrackerException</c>.
    /// </summary>
    void Execute(IReadOnlyList<string> arguments);
}
