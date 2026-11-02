namespace TaskTrackerCli.Domain;

/// <summary>
/// A single to-do entry. Immutable: every change produces a new instance, which keeps
/// <c>updatedAt</c> bookkeeping in exactly one place.
/// </summary>
public sealed record TaskItem(
    int Id,
    string Description,
    TodoStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt)
{
    public static TaskItem Create(int id, string description, DateTimeOffset now) =>
        new(id, NormalizeDescription(description), TodoStatus.Todo, now, now);

    public TaskItem WithDescription(string description, DateTimeOffset now) =>
        this with { Description = NormalizeDescription(description), UpdatedAt = now };

    public TaskItem WithStatus(TodoStatus status, DateTimeOffset now) =>
        this with { Status = status, UpdatedAt = now };

    /// <summary>Trims the description and rejects blank ones.</summary>
    public static string NormalizeDescription(string? description)
    {
        var normalized = description?.Trim();
        if (string.IsNullOrEmpty(normalized))
        {
            throw new InvalidDescriptionException("The task description must not be empty.");
        }

        return normalized;
    }
}
