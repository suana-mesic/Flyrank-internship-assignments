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
}