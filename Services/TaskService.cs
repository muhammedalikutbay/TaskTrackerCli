using TaskTrackerCli.Models;

namespace TaskTrackerCli.Services;

public class TaskService
{
    private readonly JsonFileService _jsonService;

    public TaskService(JsonFileService jsonService)
    {
        _jsonService = jsonService;
    }

    public TaskItem AddTask(string description)
    {
        var tasks = _jsonService.ReadTasks();

        var newTask = new TaskItem
        {
            Id = GenerateId(tasks),
            Description = description,
            Status = "todo",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        tasks.Add(newTask);

        _jsonService.WriteTasks(tasks);

        return newTask;
    }

    private int GenerateId(List<TaskItem> tasks)
    {
        if (tasks.Count == 0)
            return 1;

        return tasks.Max(t => t.Id) + 1;
    }
}