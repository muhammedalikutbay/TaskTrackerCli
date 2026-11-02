using System.Text.Json;
using TaskTrackerCli.Cli;
using TaskTrackerCli.Domain;
using TaskTrackerCli.Tests.Support;

namespace TaskTrackerCli.Tests;

/// <summary>Drives the whole stack (parsing → service → JSON file → output) like a real user would.</summary>
public sealed class CliEndToEndTests : IDisposable
{
    private readonly TempDirectory _dir = new();
    private readonly FakeOutput _output = new();
    private readonly FakeClock _clock = new(new DateTimeOffset(2026, 5, 6, 7, 8, 9, TimeSpan.Zero));

    public void Dispose() => _dir.Dispose();

    private string FilePath => _dir.File("tasks.json");

    private int Run(params string[] args) =>
        CompositionRoot.Create(FilePath, _output, _clock, new TaskTableFormatter(TimeZoneInfo.Utc)).Run(args);

    [Fact]
    public void FullLifecycle()
    {
        Assert.Equal(0, Run("add", "Buy groceries"));
        Assert.Equal(0, Run("add", "Write report"));
        Assert.Equal(0, Run("add", "Clean house"));
        Assert.Equal("Task added successfully (ID: 1)", _output.Lines[0]);

        _clock.Advance(TimeSpan.FromHours(1));
        Assert.Equal(0, Run("update", "1", "Buy groceries and milk"));
        Assert.Equal(0, Run("mark-in-progress", "2"));
        Assert.Equal(0, Run("mark-done", "3"));
        Assert.Equal(0, Run("delete", "3"));

        _output.Lines.Clear();
        Assert.Equal(0, Run("list"));
        var listing = _output.Text;
        Assert.Contains("Buy groceries and milk", listing);
        Assert.Contains("Write report", listing);
        Assert.DoesNotContain("Clean house", listing);

        _output.Lines.Clear();
        Assert.Equal(0, Run("list", "in-progress"));
        Assert.Contains("Write report", _output.Text);
        Assert.DoesNotContain("Buy groceries", _output.Text);

        _output.Lines.Clear();
        Assert.Equal(0, Run("list", "done"));
        Assert.Equal("No tasks found.", _output.Text);

        _output.Lines.Clear();
        Assert.Equal(0, Run("list", "todo"));
        Assert.Contains("Buy groceries and milk", _output.Text);
    }

    [Fact]
    public void UpdatingATask_RefreshesUpdatedAtButNotCreatedAt()
    {
        Run("add", "x");
        var created = _clock.Now;
        _clock.Advance(TimeSpan.FromDays(1));
        Run("mark-done", "1");

        using var doc = JsonDocument.Parse(File.ReadAllText(FilePath));
        var item = doc.RootElement[0];
        Assert.Equal(created, item.GetProperty("createdAt").GetDateTimeOffset());
        Assert.Equal(_clock.Now, item.GetProperty("updatedAt").GetDateTimeOffset());
        Assert.Equal("done", item.GetProperty("status").GetString());
    }

    [Fact]
    public void Add_WithUnquotedWords_KeepsAllOfThem()
    {
        Run("add", "Buy", "some", "milk");

        Run("list");
        Assert.Contains("Buy some milk", _output.Text);
    }

    [Fact]
    public void MarkTodo_ResetsStatus()
    {
        Run("add", "x");
        Run("mark-done", "1");
        Assert.Equal(0, Run("mark-todo", "1"));

        _output.Lines.Clear();
        Run("list", "todo");
        Assert.Contains("x", _output.Text);
    }

    [Fact]
    public void ListNotDone_CombinesTodoAndInProgress()
    {
        Run("add", "a"); Run("add", "b"); Run("add", "c");
        Run("mark-in-progress", "2"); Run("mark-done", "3");
        _output.Lines.Clear();

        Run("list", "not-done");

        var lines = _output.Text.Split(Environment.NewLine);
        Assert.Equal(3, lines.Length); // header + a + b
    }

    [Theory]
    [InlineData("update", "99", "x")]
    [InlineData("delete", "99")]
    [InlineData("mark-done", "99")]
    [InlineData("mark-in-progress", "99")]
    public void UnknownTask_FailsWithExitCode1(params string[] args)
    {
        Assert.Equal(ExitCodes.Failure, Run(args));
        Assert.Contains("not found", _output.ErrorText);
    }

    [Theory]
    [InlineData("frobnicate")]
    [InlineData("add")]
    [InlineData("update")]
    [InlineData("update", "1")]
    [InlineData("delete")]
    [InlineData("delete", "1", "2")]
    [InlineData("delete", "abc")]
    [InlineData("delete", "-1")]
    [InlineData("delete", "0")]
    [InlineData("delete", "1.5")]
    [InlineData("delete", "99999999999999999999")]
    [InlineData("mark-done")]
    [InlineData("mark-done", "x")]
    [InlineData("list", "bogus")]
    [InlineData("list", "done", "extra")]
    public void BadInput_FailsWithUsageExitCode(params string[] args)
    {
        Assert.Equal(ExitCodes.Usage, Run(args));
        Assert.NotEmpty(_output.Errors);
        Assert.Empty(_output.Lines);
    }

    [Fact]
    public void BlankDescription_IsRejectedAndNothingIsStored()
    {
        Assert.Equal(ExitCodes.Failure, Run("add", "   "));

        Run("list");
        Assert.Equal("No tasks found.", _output.Text);
    }

    [Fact]
    public void NoArguments_PrintsHelpToErrorWithUsageCode()
    {
        Assert.Equal(ExitCodes.Usage, Run());
        Assert.Contains("Usage:", _output.ErrorText);
    }

    [Theory]
    [InlineData("help")]
    [InlineData("--help")]
    [InlineData("-h")]
    public void Help_ListsEveryCommand(string flag)
    {
        Assert.Equal(ExitCodes.Success, Run(flag));
        foreach (var name in new[] { "add", "update", "delete", "mark-in-progress", "mark-done", "list" })
        {
            Assert.Contains(name, _output.Text);
        }
    }

    [Fact]
    public void CommandNames_AreCaseInsensitive()
    {
        Assert.Equal(0, Run("ADD", "x"));
        Assert.Equal(0, Run("List", "DONE"));
    }

    [Fact]
    public void CorruptFile_ReportsErrorAndKeepsFile()
    {
        File.WriteAllText(FilePath, "{broken");

        Assert.Equal(ExitCodes.Failure, Run("add", "x"));
        Assert.Contains("not a valid task file", _output.ErrorText);
        Assert.Equal("{broken", File.ReadAllText(FilePath));
    }

    [Fact]
    public void DescriptionsWithSpecialCharacters_SurviveTheFile()
    {
        const string text = "Quote \" backslash \\ unicode çğıöşü 日本語 emoji 🚀";
        Run("add", text);

        using var doc = JsonDocument.Parse(File.ReadAllText(FilePath));
        Assert.Equal(text, doc.RootElement[0].GetProperty("description").GetString());
    }

    [Fact]
    public void Registry_RejectsDuplicateCommandNames()
    {
        var command = new Cli.Commands.AddCommand(new Application.TaskService(new InMemoryTaskRepository(), _clock), _output);

        Assert.Throws<ArgumentException>(() => new CommandRegistry([command, command]));
    }
}
