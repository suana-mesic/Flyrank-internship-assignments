using Npgsql;
using System.Globalization;

namespace ImageApi.Repositories
{
    public class VectorRepository
    {
        private readonly string _connStr;
        public VectorRepository(string connStr) => _connStr = connStr;

        // Formats a float[] as a pgvector literal: "[0.1,0.2,...]".
        // InvariantCulture is mandatory so the decimal separator is always a dot.
        private static string ToVectorLiteral(float[] v) =>
            "[" + string.Join(",", v.Select(x => x.ToString(CultureInfo.InvariantCulture))) + "]";

        public void SaveImageVector (int imageId, float[] embedding)
        {
            using var conn = new NpgsqlConnection(_connStr);
            conn.Open();

            using var cmd = new NpgsqlCommand("""
            INSERT INTO image_vectors (image_id, embedding)
            VALUES (@id, @vec::vector)
            ON CONFLICT (image_id) DO UPDATE
                SET embedding = EXCLUDED.embedding, created_at = now()
            """, conn);

            cmd.Parameters.AddWithValue("id", imageId);
            cmd.Parameters.AddWithValue("vec", ToVectorLiteral(embedding));
            cmd.ExecuteNonQuery();
        }

        public void SavePostVector(int postId, float[] embedding)
        {
            using var conn = new NpgsqlConnection(_connStr);
            conn.Open();
            using var cmd = new NpgsqlCommand("""
            INSERT INTO post_vectors (post_id, embedding)
            VALUES (@id, @vec::vector)
            ON CONFLICT (post_id) DO UPDATE
                SET embedding = EXCLUDED.embedding, created_at = now()
            """, conn);
            cmd.Parameters.AddWithValue("id", postId);
            cmd.Parameters.AddWithValue("vec", ToVectorLiteral(embedding));
            cmd.ExecuteNonQuery();
        }
    }
}
