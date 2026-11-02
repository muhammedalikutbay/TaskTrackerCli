using System.Text;
using System.Text.Json;
using TaskTrackerCli.Application;
using TaskTrackerCli.Domain;

namespace TaskTrackerCli.Infrastructure;

/// <summary>
/// Stores tasks as a JSON array in a single file. The file is created on first use.
/// Writes go to a temporary file that then replaces the real one, so a crash in the
/// middle of a save can never leave a half-written task list behind.
/// </summary>
public sealed class JsonFileTaskRepository(string filePath) : ITaskRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        RespectNullableAnnotations = true,
        RespectRequiredConstructorParameters = true,
        Converters = { new TodoStatusJsonConverter() }
    };

    public IReadOnlyList<TaskItem> GetAll() => Load();

    public TaskItem? Find(int id) => Load().FirstOrDefault(t => t.Id == id);

    public TaskItem Add(Func<int, TaskItem> factory)
    {
        var tasks = Load();
        var id = tasks.Count == 0 ? 1 : tasks.Max(t => t.Id) + 1;
        var task = factory(id);

        tasks.Add(task);
        Save(tasks);
        return task;
    }

    public bool Update(TaskItem task)
    {
        var tasks = Load();
        var index = tasks.FindIndex(t => t.Id == task.Id);
        if (index < 0)
        {
            return false;
        }

        tasks[index] = task;
        Save(tasks);
        return true;
    }

    public bool Remove(int id)
    {
        var tasks = Load();
        if (tasks.RemoveAll(t => t.Id == id) == 0)
        {
            return false;
        }

        Save(tasks);
        return true;
    }

    private List<TaskItem> Load()
    {
        string json;
        try
        {
            if (!File.Exists(filePath))
            {
                Save([]);
                return [];
            }

            json = File.ReadAllText(filePath, Encoding.UTF8);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new StorageException($"Could not read '{filePath}': {ex.Message}", ex);
        }

        // An empty file is treated like a brand new one.
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            var tasks = JsonSerializer.Deserialize<List<TaskItem?>>(json, JsonOptions)
                        ?? throw new JsonException("The file contains 'null' instead of a list of tasks.");
            return Validate(tasks);
        }
        catch (JsonException ex)
        {
            throw new StorageException(
                $"'{filePath}' is not a valid task file ({ex.Message}). " +
                "Fix or delete the file; it has been left untouched.", ex);
        }
    }

    private static List<TaskItem> Validate(List<TaskItem?> tasks)
    {
        var seen = new HashSet<int>();
        var result = new List<TaskItem>(tasks.Count);

        foreach (var task in tasks)
        {
            if (task is null)
            {
                throw new JsonException("the list contains a null entry.");
            }

            if (task.Id <= 0)
            {
                throw new JsonException($"task id {task.Id} is invalid (ids start at 1).");
            }

            if (!seen.Add(task.Id))
            {
                throw new JsonException($"task id {task.Id} appears more than once.");
            }

            result.Add(task);
        }

        return result;
    }

    private void Save(IReadOnlyList<TaskItem> tasks)
    {
        var tempPath = $"{filePath}.{Guid.NewGuid():N}.tmp";
        try
        {
            var directory = Path.GetDirectoryName(Path.GetFullPath(filePath));
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(tasks, JsonOptions);
            File.WriteAllText(tempPath, json, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            File.Move(tempPath, filePath, overwrite: true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new StorageException($"Could not write '{filePath}': {ex.Message}", ex);
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                try { File.Delete(tempPath); } catch (IOException) { /* best effort cleanup */ }
            }
        }
    }
}
