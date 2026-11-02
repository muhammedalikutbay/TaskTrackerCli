namespace TaskTrackerCli.Cli;

/// <summary>Where the CLI writes to. Swapped for an in-memory fake in tests.</summary>
public interface IOutput
{
    void WriteLine(string text);

    void WriteError(string text);
}

public sealed class ConsoleOutput : IOutput
{
    public void WriteLine(string text) => Console.Out.WriteLine(text);

    public void WriteError(string text) => Console.Error.WriteLine(text);
}
