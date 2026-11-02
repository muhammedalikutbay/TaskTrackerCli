using TaskTrackerCli.Application;
using TaskTrackerCli.Cli;
using TaskTrackerCli.Domain;

namespace TaskTrackerCli.Tests.Support;

public sealed class FakeClock(DateTimeOffset start) : IClock
{
    public DateTimeOffset Now { get; set; } = start;

    public void Advance(TimeSpan by) => Now += by;
}

public sealed class FakeOutput : IOutput
{
    public List<string> Lines { get; } = [];
    public List<string> Errors { get; } = [];

    public string Text => string.Join(Environment.NewLine, Lines);
    public string ErrorText => string.Join(Environment.NewLine, Errors);

    public void WriteLine(string text) => Lines.Add(text);
    public void WriteError(string text) => Errors.Add(text);
}

public sealed class InMemoryTaskRepository : ITaskRepository
{
    private readonly List<TaskItem> _tasks = [];

    public IReadOnlyList<TaskItem> GetAll() => _tasks.ToList();

    public TaskItem? Find(int id) => _tasks.FirstOrDefault(t => t.Id == id);

    public TaskItem Add(Func<int, TaskItem> factory)
    {
        var task = factory(_tasks.Count == 0 ? 1 : _tasks.Max(t => t.Id) + 1);
        _tasks.Add(task);
        return task;
    }

    public bool Update(TaskItem task)
    {
        var index = _tasks.FindIndex(t => t.Id == task.Id);
        if (index < 0) return false;
        _tasks[index] = task;
        return true;
    }

    public bool Remove(int id) => _tasks.RemoveAll(t => t.Id == id) > 0;
}

/// <summary>A unique temp directory that is removed when disposed.</summary>
public sealed class TempDirectory : IDisposable
{
    public string Path { get; } = System.IO.Path.Combine(
        System.IO.Path.GetTempPath(), "tasktracker-tests-" + Guid.NewGuid().ToString("N"));

    public TempDirectory() => Directory.CreateDirectory(Path);

    public string File(string name) => System.IO.Path.Combine(Path, name);

    public void Dispose()
    {
        if (Directory.Exists(Path)) Directory.Delete(Path, recursive: true);
    }
}
