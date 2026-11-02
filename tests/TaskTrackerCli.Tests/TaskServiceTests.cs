using TaskTrackerCli.Application;
using TaskTrackerCli.Domain;
using TaskTrackerCli.Tests.Support;

namespace TaskTrackerCli.Tests;

public class TaskServiceTests
{
    private readonly FakeClock _clock = new(new DateTimeOffset(2026, 1, 1, 9, 0, 0, TimeSpan.Zero));
    private readonly TaskService _service;

    public TaskServiceTests() => _service = new TaskService(new InMemoryTaskRepository(), _clock);

    [Fact]
    public void Add_AssignsSequentialIdsAndTimestamps()
    {
        var first = _service.Add("one");
        _clock.Advance(TimeSpan.FromMinutes(1));
        var second = _service.Add("two");

        Assert.Equal(1, first.Id);
        Assert.Equal(2, second.Id);
        Assert.Equal(_clock.Now, second.CreatedAt);
        Assert.Equal(TodoStatus.Todo, second.Status);
    }

    [Fact]
    public void Add_RejectsBlankDescription() =>
        Assert.Throws<InvalidDescriptionException>(() => _service.Add("  "));

    [Fact]
    public void Ids_AreNotReusedWhileHigherIdsExist()
    {
        _service.Add("a");
        _service.Add("b");
        _service.Delete(1);

        Assert.Equal(3, _service.Add("c").Id);
    }

    [Fact]
    public void UpdateDescription_ChangesTextAndUpdatedAt()
    {
        var created = _service.Add("old");
        _clock.Advance(TimeSpan.FromHours(2));

        var updated = _service.UpdateDescription(created.Id, "new");

        Assert.Equal("new", updated.Description);
        Assert.Equal(created.CreatedAt, updated.CreatedAt);
        Assert.Equal(_clock.Now, updated.UpdatedAt);
        Assert.Equal("new", _service.List(TaskFilter.All).Single().Description);
    }

    [Fact]
    public void SetStatus_PersistsChange()
    {
        var task = _service.Add("a");

        _service.SetStatus(task.Id, TodoStatus.InProgress);

        Assert.Equal(TodoStatus.InProgress, _service.List(TaskFilter.All).Single().Status);
    }

    [Fact]
    public void UnknownId_ThrowsNotFound()
    {
        Assert.Equal(42, Assert.Throws<TaskNotFoundException>(() => _service.UpdateDescription(42, "x")).TaskId);
        Assert.Throws<TaskNotFoundException>(() => _service.SetStatus(42, TodoStatus.Done));
        Assert.Throws<TaskNotFoundException>(() => _service.Delete(42));
    }

    [Fact]
    public void Delete_RemovesTask()
    {
        var a = _service.Add("a");
        _service.Add("b");

        _service.Delete(a.Id);

        Assert.Equal(["b"], _service.List(TaskFilter.All).Select(t => t.Description));
    }

    [Fact]
    public void List_AppliesFilterAndOrdersById()
    {
        _service.Add("a");
        _service.Add("b");
        _service.Add("c");
        _service.SetStatus(3, TodoStatus.Done);
        _service.SetStatus(1, TodoStatus.Done);

        Assert.Equal([1, 3], _service.List(TaskFilter.Done).Select(t => t.Id));
        Assert.Equal([2], _service.List(TaskFilter.NotDone).Select(t => t.Id));
    }
}
