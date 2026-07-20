# Containerize Your Stack

**Track:** Backend AI Engineering | **Week:** 3 | **Phase:** Foundations
**Intern:** Suana Mešić

An ASP.NET Core (.NET 9) Items API with Postgres in Docker. App and database start together with one command, and the data survives a restart.

---

## The swap: one line

Both repositories implement the same `IItemRepository` interface. Swapping storage is one line in `Program.cs`:

```csharp
// builder.Services.AddSingleton<IItemRepository, InMemoryItemRepository>();
builder.Services.AddSingleton<IItemRepository, PostgresItemRepository>();
```

**`ItemService.cs` and the routes did not change.** The service depends only on the interface, so it never learns whether the data lives in a dictionary or in Postgres. That is the layering paying off.

---

## Configuration

- `.env` — real values, **gitignored**
- `.env.example` — committed template with the variable names

`docker compose` reads `.env` automatically. For local runs (`dotnet run`), the app loads it with the `DotNetEnv` package, since .NET does not read `.env` files on its own.

The connection string differs by context:
- local: `Host=localhost`
- in Docker: `Host=db` — `db` is the compose service name, which works as a DNS name inside the compose network

The compose file overrides the host through `environment:`, so the same `.env` works for both.

---

## The table

`ItemsApi/Database/init.sql` holds the `CREATE TABLE IF NOT EXISTS`. `DatabaseInitializer` runs it at startup, before the app accepts requests.

I chose this over mounting the SQL into `/docker-entrypoint-initdb.d/` so the SQL lives inside the project and is visible in Visual Studio. The trade-off, honestly: `IF NOT EXISTS` cannot alter an existing table, so a real project would need migrations. For one table that does not change, this is enough.

---

## Run it

```
docker compose up --build
```

- API: `http://localhost:8080`
- Postgres: `localhost:5432`

`depends_on` with a healthcheck makes the API wait until Postgres is actually ready — without it the API starts too early and crashes.

### Endpoints

| Method | Route |
|--------|-------|
| GET | `/` |
| GET | `/items` |
| GET | `/items/{id}` |
| POST | `/items` |
| DELETE | `/items/{id}` |

---

## Persistence proof

How I checked:

```
docker compose up --build

curl.exe -X POST http://localhost:8080/items -H "Content-Type: application/json" -d "{\"name\":\"prvi\"}"
curl.exe http://localhost:8080/items        -> row returned

docker compose down                          -> containers deleted
docker compose up -d
curl.exe http://localhost:8080/items        -> same row still there
```

The rows survive because the data lives in the named volume `pgdata`, not inside the container. `docker compose down` deletes the containers but leaves the volume. (`docker compose down -v` would delete the volume too — that is the one command that wipes the data.)

Running the same test against `InMemoryItemRepository` loses everything on restart. That is the contrast this assignment is about.

---

## Files

```
Containerize-your-stack/
├─ ItemsApi/
│  ├─ Database/
│  │  ├─ DatabaseInitializer.cs         runs init.sql at startup
│  │  └─ init.sql                       CREATE TABLE
│  ├─ Models/
│  │  └─ Item.cs
│  ├─ Repositories/
│  │  ├─ IItemRepository.cs             the interface both repos implement
│  │  ├─ InMemoryItemRepository.cs      what was replaced
│  │  └─ PostgresItemRepository.cs      what replaced it
│  ├─ Services/
│  │  └─ ItemService.cs                 unchanged by the swap
│  ├─ Properties/launchSettings.json
│  ├─ Program.cs                        routes + the one swap line
│  ├─ ItemsApi.csproj
│  ├─ appsettings.json
│  ├─ appsettings.Development.json
│  ├─ Dockerfile
│  └─ .dockerignore
├─ docker-compose.yml
├─ ItemsApi.sln
├─ .env.example                          committed
├─ .gitignore                            ignores .env, bin/, obj/, .vs/
└─ README.md
```

