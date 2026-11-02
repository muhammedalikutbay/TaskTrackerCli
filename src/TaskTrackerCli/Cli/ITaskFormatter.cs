using System.Text;
using TaskTrackerCli.Domain;

namespace TaskTrackerCli.Cli;

/// <summary>Presentation of tasks, kept out of the commands so they only orchestrate.</summary>
public interface ITaskFormatter
{
    string FormatTable(IReadOnlyList<TaskItem> tasks);
}

/// <summary>Renders tasks as an aligned plain-text table. Timestamps are shown in <paramref name="timeZone"/>.</summary>
public sealed class TaskTableFormatter(TimeZoneInfo timeZone) : ITaskFormatter
{
    private const string TimeFormat = "yyyy-MM-dd HH:mm";
    private static readonly string[] Headers = ["ID", "STATUS", "CREATED", "UPDATED", "DESCRIPTION"];

    public TaskTableFormatter() : this(TimeZoneInfo.Local) { }

    public string FormatTable(IReadOnlyList<TaskItem> tasks)
    {
        var rows = tasks.Select(t => new[]
        {
            t.Id.ToString(System.Globalization.CultureInfo.InvariantCulture),
            t.Status.ToName(),
            Format(t.CreatedAt),
            Format(t.UpdatedAt),
            SingleLine(t.Description)
        }).ToList();

        // The last column (description) is never padded, so long text cannot break alignment.
        var widths = Headers.Select((h, i) => Math.Max(h.Length, rows.Select(r => r[i].Length).DefaultIfEmpty(0).Max())).ToArray();

        var builder = new StringBuilder();
        AppendRow(builder, Headers, widths);
        foreach (var row in rows)
        {
            builder.AppendLine();
            AppendRow(builder, row, widths);
        }

        return builder.ToString();
    }

    private string Format(DateTimeOffset value) =>
        TimeZoneInfo.ConvertTime(value, timeZone).ToString(TimeFormat, System.Globalization.CultureInfo.InvariantCulture);

    private static string SingleLine(string text) =>
        text.Replace("\r\n", " ").Replace('\n', ' ').Replace('\r', ' ');

    private static void AppendRow(StringBuilder builder, string[] cells, int[] widths)
    {
        for (var i = 0; i < cells.Length - 1; i++)
        {
            builder.Append(cells[i].PadRight(widths[i])).Append("  ");
        }

        builder.Append(cells[^1]);
    }
}
