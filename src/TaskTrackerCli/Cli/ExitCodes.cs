namespace TaskTrackerCli.Cli;

public static class ExitCodes
{
    public const int Success = 0;

    /// <summary>The command was understood but could not be carried out.</summary>
    public const int Failure = 1;

    /// <summary>The command line itself was wrong (unknown command, bad arguments).</summary>
    public const int Usage = 2;
}
