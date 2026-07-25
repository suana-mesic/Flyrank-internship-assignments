using Npgsql;

namespace ImageApi.Repositories;

public class MatchRepository
{
    private readonly string _connStr;
    public MatchRepository(string connStr) => _connStr = connStr;

    // Ranks images for a post by cosine similarity (1 - cosine distance).
    // <=> is pgvector's cosine-distance operator; ORDER BY it ASC = nearest first.
    public List<(int imageId, string filename, string subject, double similarity)> RankImagesForPost(int postId, int limit)
    {
        using var conn = new NpgsqlConnection(_connStr);
        conn.Open();
        using var cmd = new NpgsqlCommand("""
            SELECT i.id, i.filename, it.subject,
                   1 - (iv.embedding <=> pv.embedding) AS similarity
            FROM post_vectors pv
            CROSS JOIN image_vectors iv
            JOIN images i      ON i.id = iv.image_id
            JOIN image_tags it ON it.image_id = iv.image_id
            WHERE pv.post_id = @pid
            ORDER BY iv.embedding <=> pv.embedding ASC
            LIMIT @lim
            """, conn);
        cmd.Parameters.AddWithValue("pid", postId);
        cmd.Parameters.AddWithValue("lim", limit);

        var list = new List<(int, string, string, double)>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            list.Add((reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetDouble(3)));
        return list;
    }
}