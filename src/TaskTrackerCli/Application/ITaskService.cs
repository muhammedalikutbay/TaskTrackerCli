using TaskTrackerCli.Domain;

namespace TaskTrackerCli.Application;

/// <summary>Read side of the task use cases.</summary>
public interface ITaskQueryService
{
    IReadOnlyList<TaskItem> List(TaskFilter filter);
}

/// <summary>Write side of the task use cases.</summary>
public interface ITaskCommandService
{
    TaskItem Add(string description);

    TaskItem UpdateDescription(int id, string description);

    TaskItem SetStatus(int id, TodoStatus status);

    void Delete(int id);
}
