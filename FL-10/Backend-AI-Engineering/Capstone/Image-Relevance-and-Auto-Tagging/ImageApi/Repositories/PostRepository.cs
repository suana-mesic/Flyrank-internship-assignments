using Npgsql;

namespace ImageApi.Repositories;

public class PostRepository
{
    private readonly string _connStr;
    public PostRepository(string connStr) => _connStr = connStr;

    // Upserts a post; on existing slug it updates fields (so re-seeding backfills topic).
    public bool Insert(string slug, string title, string body, string topic)
    {
        using var conn = new NpgsqlConnection(_connStr);
        conn.Open();
        using var cmd = new NpgsqlCommand("""
            INSERT INTO posts (slug, title, body, topic)
            VALUES (@s, @t, @b, @topic)
            ON CONFLICT (slug) DO UPDATE
                SET title = EXCLUDED.title, body = EXCLUDED.body, topic = EXCLUDED.topic
            """, conn);
        cmd.Parameters.AddWithValue("s", slug);
        cmd.Parameters.AddWithValue("t", title);
        cmd.Parameters.AddWithValue("b", body);
        cmd.Parameters.AddWithValue("topic", topic);
        return cmd.ExecuteNonQuery() > 0;
    }

    // Fetches one post (id, slug, title, topic).
    public (int id, string slug, string title, string topic)? GetById(int id)
    {
        using var conn = new NpgsqlConnection(_connStr);
        conn.Open();
        using var cmd = new NpgsqlCommand(
            "SELECT id, slug, title, COALESCE(topic,'') FROM posts WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("id", id);
        using var reader = cmd.ExecuteReader();
        if (!reader.Read()) return null;
        return (reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetString(3));
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