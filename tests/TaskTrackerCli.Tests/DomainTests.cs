using TaskTrackerCli.Domain;

namespace TaskTrackerCli.Tests;

public class DomainTests
{
    private static readonly DateTimeOffset T0 = new(2026, 1, 1, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_StartsAsTodoWithMatchingTimestamps()
    {
        var task = TaskItem.Create(7, "  Write tests  ", T0);

        Assert.Equal(7, task.Id);
        Assert.Equal("Write tests", task.Description);
        Assert.Equal(TodoStatus.Todo, task.Status);
        Assert.Equal(T0, task.CreatedAt);
        Assert.Equal(T0, task.UpdatedAt);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_RejectsBlankDescription(string? description) =>
        Assert.Throws<InvalidDescriptionException>(() => TaskItem.Create(1, description!, T0));

    [Fact]
    public void WithStatus_ChangesStatusAndUpdatedAtOnly()
    {
        var original = TaskItem.Create(1, "x", T0);
        var later = T0.AddHours(1);

        var changed = original.WithStatus(TodoStatus.Done, later);

        Assert.Equal(TodoStatus.Done, changed.Status);
        Assert.Equal(later, changed.UpdatedAt);
        Assert.Equal(T0, changed.CreatedAt);
        Assert.Equal(TodoStatus.Todo, original.Status); // immutable
    }

    [Fact]
    public void WithDescription_ValidatesAndTouchesUpdatedAt()
    {
        var original = TaskItem.Create(1, "x", T0);

        var changed = original.WithDescription(" y ", T0.AddMinutes(5));

        Assert.Equal("y", changed.Description);
        Assert.Equal(T0.AddMinutes(5), changed.UpdatedAt);
        Assert.Throws<InvalidDescriptionException>(() => original.WithDescription(" ", T0));
    }

    [Theory]
    [InlineData("todo", TodoStatus.Todo)]
    [InlineData("In-Progress", TodoStatus.InProgress)]
    [InlineData(" done ", TodoStatus.Done)]
    public void StatusNames_RoundTrip(string text, TodoStatus expected)
    {
        Assert.True(TodoStatusNames.TryParse(text, out var status));
        Assert.Equal(expected, status);
        Assert.True(TodoStatusNames.TryParse(status.ToName(), out var again));
        Assert.Equal(expected, again);
    }

    [Theory]
    [InlineData("")]
    [InlineData("finished")]
    [InlineData(null)]
    public void StatusNames_RejectUnknown(string? text) =>
        Assert.False(TodoStatusNames.TryParse(text, out _));

    [Fact]
    public void Filters_SelectExpectedTasks()
    {
        var todo = TaskItem.Create(1, "a", T0);
        var doing = TaskItem.Create(2, "b", T0).WithStatus(TodoStatus.InProgress, T0);
        var done = TaskItem.Create(3, "c", T0).WithStatus(TodoStatus.Done, T0);
        TaskItem[] all = [todo, doing, done];

        Assert.Equal(all, all.Where(TaskFilter.All.Matches));
        Assert.Equal([todo], all.Where(TaskFilter.Todo.Matches));
        Assert.Equal([doing], all.Where(TaskFilter.InProgress.Matches));
        Assert.Equal([done], all.Where(TaskFilter.Done.Matches));
        Assert.Equal([todo, doing], all.Where(TaskFilter.NotDone.Matches));
    }

    [Fact]
    public void FilterTryParse_IsCaseInsensitiveAndRejectsUnknown()
    {
        Assert.True(TaskFilter.TryParse("DONE", out var filter));
        Assert.Same(TaskFilter.Done, filter);
        Assert.False(TaskFilter.TryParse("nope", out _));
    }
}
