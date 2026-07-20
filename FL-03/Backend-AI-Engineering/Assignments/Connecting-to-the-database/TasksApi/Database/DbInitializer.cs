using Microsoft.Data.Sqlite;

namespace TasksApi.Database;

public static class DbInitializer
{
    public static void Initialize(string connectionString)
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        var createTable = connection.CreateCommand();
        createTable.CommandText = """
            CREATE TABLE IF NOT EXISTS tasks (
                id    INTEGER PRIMARY KEY AUTOINCREMENT,
                title TEXT NOT NULL,
                done  INTEGER NOT NULL DEFAULT 0
            );
            """;
        createTable.ExecuteNonQuery();

        var count = connection.CreateCommand();
        count.CommandText = "SELECT COUNT(*) FROM tasks;";
        var taskCount = (long)count.ExecuteScalar()!;

        if (taskCount == 0)
        {
            var seed = connection.CreateCommand();
            seed.CommandText = """
                INSERT INTO tasks (title, done) VALUES ('Buy groceries', 0);
                INSERT INTO tasks (title, done) VALUES ('Read a book', 0);
                INSERT INTO tasks (title, done) VALUES ('Write assignment', 1);
                """;
            seed.ExecuteNonQuery();

            Console.WriteLine("Seeded 3 example tasks.");
        }
    }
}