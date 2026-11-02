namespace TaskTrackerCli.Cli;

/// <summary>Turns raw positional arguments into typed values, or a helpful <see cref="UsageException"/>.</summary>
public static class ArgumentParser
{
    public static int ParseId(string? text)
    {
        if (!int.TryParse(text, System.Globalization.NumberStyles.None,
                System.Globalization.CultureInfo.InvariantCulture, out var id) || id <= 0)
        {
            throw new UsageException($"'{text}' is not a valid task ID. IDs are positive whole numbers.");
        }

        return id;
    }

    public static void RequireExactly(IReadOnlyList<string> arguments, int count, string usage)
    {
        if (arguments.Count != count)
        {
            throw new UsageException($"Expected {count} argument(s), got {arguments.Count}. Usage: {usage}");
        }
    }

    /// <summary>
    /// Joins the arguments into one description. Quoting is recommended, but
    /// <c>add Buy milk</c> works too instead of silently dropping "milk".
    /// </summary>
    public static string JoinText(IEnumerable<string> arguments) => string.Join(' ', arguments);
}
