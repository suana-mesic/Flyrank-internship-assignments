using ItemsApi.Database;
using ItemsApi.Repositories;
using ItemsApi.Services;
using Npgsql;

DotNetEnv.Env.TraversePath().Load();
var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Items")
?? throw new InvalidOperationException("Missing ConnectionStrings__Items. Copy .env.example to .env.");

builder.Services.AddSingleton(NpgsqlDataSource.Create(connectionString));

builder.Services.AddSingleton<IItemRepository, PostgresItemRepository>();
builder.Services.AddScoped<ItemService>();

var app = builder.Build();
                                                                          
await DatabaseInitializer.InitialiseAsync(app.Services.GetRequiredService<NpgsqlDataSource>());

app.MapGet("/", () => Results.Ok(new { message = "Items API is running" }));

app.MapGet("/items", async (ItemService service, CancellationToken ct)
    => Results.Ok(await service.GetAllAsync(ct)));

app.MapGet("/items/{id:guid}", async (Guid id, ItemService service, CancellationToken ct) =>
{
    var item = await service.GetByIdAsync(id, ct);
    return item is null ? Results.NotFound() : Results.Ok(item);
});

app.MapPost("/items", async (CreateItemRequest request, ItemService service, CancellationToken ct) =>
{
    var item = await service.CreateAsync(request.Name, ct);
    return Results.Created($"/items/{item.Id}", item);
});

app.MapDelete("/items/{id:guid}", async (Guid id, ItemService service, CancellationToken ct)
    => await service.DeleteAsync(id, ct) ? Results.NoContent() : Results.NotFound());

app.Run();

public sealed record CreateItemRequest(string Name);