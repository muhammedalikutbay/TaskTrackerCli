using System.Text.Json;
using TaskTrackerCli.Domain;
using TaskTrackerCli.Infrastructure;
using TaskTrackerCli.Tests.Support;

namespace TaskTrackerCli.Tests;

public sealed class JsonFileTaskRepositoryTests : IDisposable
{
    private static readonly DateTimeOffset T0 = new(2026, 1, 1, 9, 0, 0, TimeSpan.Zero);
    private readonly TempDirectory _dir = new();
    private string FilePath => _dir.File("tasks.json");

    public void Dispose() => _dir.Dispose();

    [Fact]
    public void MissingFile_IsCreatedAsEmptyList()
    {
        var repository = new JsonFileTaskRepository(FilePath);

        Assert.Empty(repository.GetAll());
        Assert.True(File.Exists(FilePath));
        Assert.Equal("[]", File.ReadAllText(FilePath).Trim());
    }

    [Fact]
    public void MissingDirectory_IsCreatedOnSave()
    {
        var path = Path.Combine(_dir.Path, "nested", "tasks.json");

        new JsonFileTaskRepository(path).Add(id => TaskItem.Create(id, "x", T0));

        Assert.True(File.Exists(path));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   \n")]
    public void BlankFile_IsTreatedAsEmpty(string content)
    {
        File.WriteAllText(FilePath, content);

        Assert.Empty(new JsonFileTaskRepository(FilePath).GetAll());
    }

    [Fact]
    public void Add_PersistsTaskWithSpecifiedJsonShape()
    {
        new JsonFileTaskRepository(FilePath).Add(id => TaskItem.Create(id, "Buy milk", T0));

        using var doc = JsonDocument.Parse(File.ReadAllText(FilePath));
        var item = doc.RootElement.EnumerateArray().Single();
        Assert.Equal(1, item.GetProperty("id").GetInt32());
        Assert.Equal("Buy milk", item.GetProperty("description").GetString());
        Assert.Equal("todo", item.GetProperty("status").GetString());
        Assert.Equal(T0, item.GetProperty("createdAt").GetDateTimeOffset());
        Assert.Equal(T0, item.GetProperty("updatedAt").GetDateTimeOffset());
    }

    [Fact]
    public void Data_SurvivesANewRepositoryInstance()
    {
        new JsonFileTaskRepository(FilePath).Add(id => TaskItem.Create(id, "persisted", T0));

        var reloaded = new JsonFileTaskRepository(FilePath).GetAll();

        Assert.Equal("persisted", Assert.Single(reloaded).Description);
    }

    [Fact]
    public void Update_And_Remove_ReportWhetherTaskExisted()
    {
        var repository = new JsonFileTaskRepository(FilePath);
        var task = repository.Add(id => TaskItem.Create(id, "a", T0));

        Assert.True(repository.Update(task.WithStatus(TodoStatus.Done, T0)));
        Assert.Equal(TodoStatus.Done, repository.Find(task.Id)!.Status);
        Assert.False(repository.Update(TaskItem.Create(99, "ghost", T0)));

        Assert.True(repository.Remove(task.Id));
        Assert.False(repository.Remove(task.Id));
        Assert.Empty(repository.GetAll());
    }

    [Fact]
    public void Add_UsesMaxIdPlusOne()
    {
        File.WriteAllText(FilePath, """
            [{"id":5,"description":"x","status":"done","createdAt":"2026-01-01T00:00:00+00:00","updatedAt":"2026-01-01T00:00:00+00:00"}]
            """);

        var added = new JsonFileTaskRepository(FilePath).Add(id => TaskItem.Create(id, "y", T0));

        Assert.Equal(6, added.Id);
    }

    [Fact]
    public void Save_LeavesNoTemporaryFilesBehind()
    {
        var repository = new JsonFileTaskRepository(FilePath);
        repository.Add(id => TaskItem.Create(id, "a", T0));
        repository.Add(id => TaskItem.Create(id, "b", T0));

        Assert.Equal([FilePath], Directory.GetFiles(_dir.Path));
    }

    [Theory]
    [InlineData("{not json")]
    [InlineData("{\"id\":1}")]
    [InlineData("null")]
    [InlineData("[null]")]
    [InlineData("[{\"id\":1,\"description\":\"x\",\"status\":\"finished\",\"createdAt\":\"2026-01-01T00:00:00Z\",\"updatedAt\":\"2026-01-01T00:00:00Z\"}]")]
    [InlineData("[{\"id\":1,\"description\":\"x\",\"status\":\"todo\"}]")]
    [InlineData("[{\"id\":0,\"description\":\"x\",\"status\":\"todo\",\"createdAt\":\"2026-01-01T00:00:00Z\",\"updatedAt\":\"2026-01-01T00:00:00Z\"}]")]
    [InlineData("[{\"id\":1,\"description\":null,\"status\":\"todo\",\"createdAt\":\"2026-01-01T00:00:00Z\",\"updatedAt\":\"2026-01-01T00:00:00Z\"}]")]
    public void CorruptFile_ThrowsStorageException_AndIsNotOverwritten(string content)
    {
        File.WriteAllText(FilePath, content);
        var repository = new JsonFileTaskRepository(FilePath);

        Assert.Throws<StorageException>(() => repository.GetAll());
        Assert.Throws<StorageException>(() => repository.Add(id => TaskItem.Create(id, "new", T0)));
        Assert.Equal(content, File.ReadAllText(FilePath));
    }

    [Fact]
    public void DuplicateIds_AreRejected()
    {
        const string item = "{\"id\":1,\"description\":\"x\",\"status\":\"todo\",\"createdAt\":\"2026-01-01T00:00:00Z\",\"updatedAt\":\"2026-01-01T00:00:00Z\"}";
        File.WriteAllText(FilePath, $"[{item},{item}]");

        Assert.Throws<StorageException>(() => new JsonFileTaskRepository(FilePath).GetAll());
    }

    [Fact]
    public void PathThatIsADirectory_ThrowsStorageException() =>
        Assert.Throws<StorageException>(() => new JsonFileTaskRepository(_dir.Path).GetAll());
}
