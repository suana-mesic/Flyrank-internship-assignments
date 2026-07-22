using ImageApi.Models;
using Npgsql;

namespace ImageApi.Repositories
{
    public class TagRepository
    {
        private readonly string _connStr;
        public TagRepository(string connStr) => _connStr = connStr;

        // Stores (or overwrites) the vision tags for one image.
        public void Save(int imageId, ImageTags tags, bool flagged, string model)
        {
            using var conn = new NpgsqlConnection(_connStr);
            conn.Open();
            using var cmd = new NpgsqlCommand("""
            INSERT INTO image_tags
                (image_id, subject, category, attributes, caption, confidence, flagged, model)
            VALUES (@iid, @sub, @cat, @attr, @cap, @conf, @flag, @model)
            ON CONFLICT (image_id) DO UPDATE SET
                subject    = EXCLUDED.subject,
                category   = EXCLUDED.category,
                attributes = EXCLUDED.attributes,
                caption    = EXCLUDED.caption,
                confidence = EXCLUDED.confidence,
                flagged    = EXCLUDED.flagged,
                model      = EXCLUDED.model
            """, conn);
            cmd.Parameters.AddWithValue("iid", imageId);
            cmd.Parameters.AddWithValue("sub", tags.Subject);
            cmd.Parameters.AddWithValue("cat", tags.Category);
            cmd.Parameters.AddWithValue("attr", tags.Attributes); // Npgsql maps string[] -> text[]
            cmd.Parameters.AddWithValue("cap", tags.Caption);
            cmd.Parameters.AddWithValue("conf", tags.Confidence);
            cmd.Parameters.AddWithValue("flag", flagged);
            cmd.Parameters.AddWithValue("model", model);
            cmd.ExecuteNonQuery();
        }
    }
}
