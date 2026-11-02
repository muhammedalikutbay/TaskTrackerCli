using System.Text.Json;
using System.Text.Json.Serialization;
using TaskTrackerCli.Domain;

namespace TaskTrackerCli.Infrastructure;

/// <summary>Reads/writes statuses as <c>todo</c>, <c>in-progress</c> and <c>done</c>.</summary>
public sealed class TodoStatusJsonConverter : JsonConverter<TodoStatus>
{
    public override TodoStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var text = reader.TokenType == JsonTokenType.String ? reader.GetString() : null;
        return TodoStatusNames.TryParse(text, out var status)
            ? status
            : throw new JsonException($"'{text}' is not a valid task status (expected todo, in-progress or done).");
    }

    public override void Write(Utf8JsonWriter writer, TodoStatus value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.ToName());
}
