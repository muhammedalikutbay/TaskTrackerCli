using TaskTrackerCli.Domain;

namespace TaskTrackerCli.Cli;

/// <summary>The user typed something the CLI cannot interpret.</summary>
public sealed class UsageException(string message) : TaskTrackerException(message);
