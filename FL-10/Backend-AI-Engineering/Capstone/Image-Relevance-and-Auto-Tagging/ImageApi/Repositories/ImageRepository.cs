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

            return cmd.ExecuteNonQuery()>0;
        }
    }
}
