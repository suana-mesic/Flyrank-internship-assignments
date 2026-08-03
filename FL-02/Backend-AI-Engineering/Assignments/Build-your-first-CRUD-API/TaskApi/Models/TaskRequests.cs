namespace TaskApi.Models;

// The client sends only a title when creating a task; id and done are set by the server.
public record CreateTaskRequest(string? Title);

// Update allows changing the title, the done flag, or both. Nulls mean "leave as is".
public record UpdateTaskRequest(string? Title, bool? Done);
