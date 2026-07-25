using ImageApi.Models;
using Npgsql;

namespace ImageApi.Repositories;

public class PairingRepository
{
    private readonly string _connStr;
    public PairingRepository(string connStr) => _connStr = connStr;

    // Replaces any existing pairing for a post with a fresh one; returns its id.
    public int Suggest(int postId, int? imageId, double? similarity, string status, string reason)
    {
        using var conn = new NpgsqlConnection(_connStr);
        conn.Open();
        using var tx = conn.BeginTransaction();

        using (var del = new NpgsqlCommand("DELETE FROM pairings WHERE post_id = @pid", conn, tx))
        {
            del.Parameters.AddWithValue("pid", postId);
            del.ExecuteNonQuery();
        }

        using var ins = new NpgsqlCommand("""
            INSERT INTO pairings (post_id, image_id, similarity, status, reason)
            VALUES (@pid, @iid, @sim, @status, @reason)
            RETURNING id
            """, conn, tx);
        ins.Parameters.AddWithValue("pid", postId);
        ins.Parameters.AddWithValue("iid", (object?)imageId ?? DBNull.Value);
        ins.Parameters.AddWithValue("sim", (object?)similarity ?? DBNull.Value);
        ins.Parameters.AddWithValue("status", status);
        ins.Parameters.AddWithValue("reason", reason);
        var id = (int)ins.ExecuteScalar()!;

        tx.Commit();
        return id;
    }

    // Approve / reject a pairing.
    public bool SetStatus(int pairingId, string status)
    {
        using var conn = new NpgsqlConnection(_connStr);
        conn.Open();
        using var cmd = new NpgsqlCommand(
            "UPDATE pairings SET status = @s, updated_at = now() WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("s", status);
        cmd.Parameters.AddWithValue("id", pairingId);
        return cmd.ExecuteNonQuery() > 0;
    }

    // All pairings joined with post + image info, for the review table.
    public List<PairingRow> GetAll()
    {
        using var conn = new NpgsqlConnection(_connStr);
        conn.Open();
        using var cmd = new NpgsqlCommand("""
            SELECT pr.id, p.slug, i.filename, it.subject, pr.similarity, pr.status, pr.reason
            FROM pairings pr
            JOIN posts p        ON p.id = pr.post_id
            LEFT JOIN images i  ON i.id = pr.image_id
            LEFT JOIN image_tags it ON it.image_id = pr.image_id
            ORDER BY p.id
            """, conn);

        var list = new List<PairingRow>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            list.Add(new PairingRow(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.IsDBNull(2) ? null : reader.GetString(2),
                reader.IsDBNull(3) ? null : reader.GetString(3),
                reader.IsDBNull(4) ? null : reader.GetDouble(4),
                reader.GetString(5),
                reader.IsDBNull(6) ? null : reader.GetString(6)));
        return list;
    }
}