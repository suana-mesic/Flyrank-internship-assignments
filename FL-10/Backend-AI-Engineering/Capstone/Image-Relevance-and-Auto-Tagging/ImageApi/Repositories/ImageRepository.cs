using Npgsql;

namespace ImageApi.Repositories
{
    public class ImageRepository
    {
        private readonly string _connStr;
        public ImageRepository(string connStr) => _connStr = connStr;

        // Inserts one image row; returns true only if a new row was actually added.
        // Idempotent: re-running ingest won't create duplicates (filename is UNIQUE).
        public bool Insert(string filename, string category)
        {
            using var conn = new NpgsqlConnection(_connStr);
            conn.Open();

            using var cmd = new NpgsqlCommand("""

                INSERT INTO images (filename, category)
                VALUES (@f, @c)
                ON CONFLICT (filename) DO NOTHING
                """, conn);
            cmd.Parameters.AddWithValue("f", filename);
            cmd.Parameters.AddWithValue("c", category);

            return cmd.ExecuteNonQuery() > 0;
        }

        // Fetches one image by id (returns null if not found).
        public (int id, string filename, string category, string status)? GetById(int id)
        {
            using var conn = new NpgsqlConnection(_connStr);
            conn.Open();
            using var cmd = new NpgsqlCommand(
                "SELECT id, filename, COALESCE(category,''), status FROM images WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("id", id);
            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return null;
            return (reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetString(3));
        }

        // Updates the classification lifecycle status: pending -> classified / failed.
        public void UpdateStatus(int id, string status)
        {
            using var conn = new NpgsqlConnection(_connStr);
            conn.Open();
            using var cmd = new NpgsqlCommand(
                "UPDATE images SET status = @s WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("s", status);
            cmd.Parameters.AddWithValue("id", id);
            cmd.ExecuteNonQuery();
        }
    }
}
