using Npgsql;

namespace ImageApi.Repositories;

public class PostRepository
{
    private readonly string _connStr;
    public PostRepository(string connStr) => _connStr = connStr;

    // Inserts one post; returns true only if a new row was added (slug is UNIQUE).
    public bool Insert(string slug, string title, string body)
    {
        using var conn = new NpgsqlConnection(_connStr);
        conn.Open();
        using var cmd = new NpgsqlCommand("""
            INSERT INTO posts (slug, title, body)
            VALUES (@s, @t, @b)
            ON CONFLICT (slug) DO NOTHING
            """, conn);
        cmd.Parameters.AddWithValue("s", slug);
        cmd.Parameters.AddWithValue("t", title);
        cmd.Parameters.AddWithValue("b", body);
        return cmd.ExecuteNonQuery() > 0;
    }

    // Returns all posts (id, title, body) to embed.
    public List<(int id, string title, string body)> GetAll()
    {
        using var conn = new NpgsqlConnection(_connStr);
        conn.Open();
        using var cmd = new NpgsqlCommand(
            "SELECT id, title, body FROM posts ORDER BY id", conn);

        var list = new List<(int, string, string)>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            list.Add((reader.GetInt32(0), reader.GetString(1), reader.GetString(2)));
        return list;
    }
}