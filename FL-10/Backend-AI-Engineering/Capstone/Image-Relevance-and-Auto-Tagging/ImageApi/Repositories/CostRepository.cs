using Npgsql;

namespace ImageApi.Repositories;

public class CostRepository
{
    private readonly string _connStr;
    public CostRepository(string connStr) => _connStr = connStr;

    // Records one AI call (vision or embedding) with its token counts and cost.
    public void Record(string operation, string model, int inputTokens, int outputTokens, decimal cost)
    {
        using var conn = new NpgsqlConnection(_connStr);
        conn.Open();
        using var cmd = new NpgsqlCommand("""
            INSERT INTO cost_events (operation, model, input_tokens, output_tokens, cost)
            VALUES (@op, @model, @in, @out, @cost)
            """, conn);
        cmd.Parameters.AddWithValue("op", operation);
        cmd.Parameters.AddWithValue("model", model);
        cmd.Parameters.AddWithValue("in", inputTokens);
        cmd.Parameters.AddWithValue("out", outputTokens);
        cmd.Parameters.AddWithValue("cost", cost);
        cmd.ExecuteNonQuery();
    }

    // Rolls up all recorded AI calls, grouped by operation (vision / embedding).
    public List<(string operation, long calls, long inputTokens, long outputTokens, decimal cost)> Summary()
    {
        using var conn = new NpgsqlConnection(_connStr);
        conn.Open();
        using var cmd = new NpgsqlCommand("""
            SELECT operation,
                   COUNT(*),
                   COALESCE(SUM(input_tokens), 0),
                   COALESCE(SUM(output_tokens), 0),
                   COALESCE(SUM(cost), 0)
            FROM cost_events
            GROUP BY operation
            ORDER BY operation
            """, conn);

        var list = new List<(string, long, long, long, decimal)>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            list.Add((reader.GetString(0), reader.GetInt64(1), reader.GetInt64(2),
                      reader.GetInt64(3), reader.GetDecimal(4)));
        return list;
    }
}