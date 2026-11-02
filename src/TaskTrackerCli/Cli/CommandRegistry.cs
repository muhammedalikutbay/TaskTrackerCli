namespace TaskTrackerCli.Cli;

/// <summary>Looks commands up by name (case-insensitive) and rejects duplicate registrations.</summary>
public sealed class CommandRegistry
{
    private readonly Dictionary<string, ICommand> _commands = new(StringComparer.OrdinalIgnoreCase);

    public CommandRegistry(IEnumerable<ICommand> commands)
    {
        foreach (var command in commands)
        {
            if (!_commands.TryAdd(command.Name, command))
            {
                throw new ArgumentException($"A command named '{command.Name}' is already registered.", nameof(commands));
            }
        }
    }

    public IReadOnlyCollection<ICommand> All => _commands.Values;

    public bool TryGet(string name, out ICommand command)
    {
        if (_commands.TryGetValue(name, out var found))
        {
            command = found;
            return true;
        }

        command = null!;
        return false;
    }
}
