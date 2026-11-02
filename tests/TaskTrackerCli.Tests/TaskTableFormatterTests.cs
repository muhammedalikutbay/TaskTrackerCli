using TaskTrackerCli.Cli;
using TaskTrackerCli.Domain;

namespace TaskTrackerCli.Tests;

public class TaskTableFormatterTests
{
    private static readonly DateTimeOffset T0 = new(2026, 3, 4, 15, 30, 0, TimeSpan.Zero);

    [Fact]
    public void FormatTable_AlignsColumnsAndLeavesDescriptionUnpadded()
    {
        var tasks = new[]
        {
            TaskItem.Create(1, "Short", T0),
            TaskItem.Create(10, "Another one", T0).WithStatus(TodoStatus.InProgress, T0.AddHours(1))
        };

        var lines = new TaskTableFormatter(TimeZoneInfo.Utc).FormatTable(tasks).Split(Environment.NewLine);

        Assert.Equal("ID  STATUS       CREATED           UPDATED           DESCRIPTION", lines[0]);
        Assert.Equal("1   todo         2026-03-04 15:30  2026-03-04 15:30  Short", lines[1]);
        Assert.Equal("10  in-progress  2026-03-04 15:30  2026-03-04 16:30  Another one", lines[2]);
    }

    [Fact]
    public void FormatTable_ConvertsToConfiguredTimeZone()
    {
        var plusTwo = TimeZoneInfo.CreateCustomTimeZone("plus2", TimeSpan.FromHours(2), "plus2", "plus2");

        var text = new TaskTableFormatter(plusTwo).FormatTable([TaskItem.Create(1, "x", T0)]);

        Assert.Contains("2026-03-04 17:30", text);
    }

    [Fact]
    public void FormatTable_KeepsMultilineDescriptionsOnOneRow()
    {
        var text = new TaskTableFormatter(TimeZoneInfo.Utc).FormatTable([TaskItem.Create(1, "a\nb\r\nc", T0)]);

        Assert.Equal(2, text.Split(Environment.NewLine).Length);
        Assert.EndsWith("a b c", text);
    }
}
