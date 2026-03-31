using System.Text.Json;
using TaskTrackerCli.Models;

namespace TaskTrackerCli.Services;

public class JsonFileService
{
    private const string FilePath = "tasks.json";

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true
    };

    public List<TaskItem> ReadTasks()
    {
        if (!File.Exists(FilePath))
            return [];

        var json = File.ReadAllText(FilePath);

        if (string.IsNullOrWhiteSpace(json))
            return [];

        return JsonSerializer.Deserialize<List<TaskItem>>(json) ?? [];
    }

    public void WriteTasks(List<TaskItem> tasks)
    {
        var json = JsonSerializer.Serialize(tasks, _jsonOptions);
        File.WriteAllText(FilePath, json);
    }
}