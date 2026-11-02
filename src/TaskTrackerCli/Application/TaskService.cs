using TaskTrackerCli.Domain;

namespace TaskTrackerCli.Application;

/// <summary>Implements the task use cases on top of an <see cref="ITaskRepository"/>.</summary>
public sealed class TaskService(ITaskRepository repository, IClock clock)
    : ITaskQueryService, ITaskCommandService
{
    public IReadOnlyList<TaskItem> List(TaskFilter filter) =>
        repository.GetAll().Where(filter.Matches).OrderBy(t => t.Id).ToList();

    public TaskItem Add(string description)
    {
        var now = clock.Now;
        return repository.Add(id => TaskItem.Create(id, description, now));
    }

    public TaskItem UpdateDescription(int id, string description) =>
        Modify(id, task => task.WithDescription(description, clock.Now));

    public TaskItem SetStatus(int id, TodoStatus status) =>
        Modify(id, task => task.WithStatus(status, clock.Now));

    public void Delete(int id)
    {
        if (!repository.Remove(id))
        {
            throw new TaskNotFoundException(id);
        }
    }

    private TaskItem Modify(int id, Func<TaskItem, TaskItem> change)
    {
        var existing = repository.Find(id) ?? throw new TaskNotFoundException(id);
        var changed = change(existing);

        if (!repository.Update(changed))
        {
            throw new TaskNotFoundException(id);
        }

        return changed;
    }
}
