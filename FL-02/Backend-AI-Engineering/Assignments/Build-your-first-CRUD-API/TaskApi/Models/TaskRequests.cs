namespace TaskApi.Models;

// The client sends only a title when creating a task; id and done are set by the server.
public record CreateTaskRequest(string? Title);
