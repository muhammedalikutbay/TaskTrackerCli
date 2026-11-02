using TaskTrackerCli.Domain;

namespace TaskTrackerCli.Application;

/// <summary>Persistence port. The application layer knows nothing about JSON or files.</summary>
public interface ITaskRepository
{
    IReadOnlyList<TaskItem> GetAll();

    TaskItem? Find(int id);

    /// <summary>
    /// Stores a new task. The repository owns identity generation, so the factory receives
    /// the freshly allocated id and returns the task to persist.
    /// </summary>
    TaskItem Add(Func<int, TaskItem> factory);

    /// <returns><c>false</c> when no task with that id exists.</returns>
    bool Update(TaskItem task);

    /// <returns><c>false</c> when no task with that id exists.</returns>
    bool Remove(int id);
}
