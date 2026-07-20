namespace ItemsApi.Models;

public sealed record Item(Guid Id, string Name, DateTime CreatedAtUtc);