using Npgsql;

namespace ImageApi.Database;

// Runs init.sql once at startup so the schema exists without manual psql steps.
public static class DatabaseInitializer
{
    public static void Initialize(string connStr)
    {
        // init.sql is copied next to the built app (see the .csproj change).
        var sqlPath = Path.Combine(AppContext.BaseDirectory, "Database", "init.sql");
        var sql = File.ReadAllText(sqlPath);

        using var conn = new NpgsqlConnection(connStr);
        conn.Open();

        // Npgsql runs the whole multi-statement script in one call.
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.ExecuteNonQuery();

        Console.WriteLine("Database initialized.");
    }
}