using Npgsql;

namespace BillingApi.Repositories;

public class UsageRepository
{
    private readonly string _connStr;

    public UsageRepository(string connStr) => _connStr = connStr;

    public bool TryRecord(int tenantId, string usageType, int quantity, string idempotencyKey)
    {
        using var conn = new NpgsqlConnection(_connStr);
        conn.Open();

        using var cmd = new NpgsqlCommand("""
            INSERT INTO usage_events (tenant_id, usage_type, quantity, idempotency_key)
            VALUES (@tid, @type, @qty, @key)
            ON CONFLICT (tenant_id, idempotency_key) DO NOTHING
            """, conn);

        cmd.Parameters.AddWithValue("tid", tenantId);
        cmd.Parameters.AddWithValue("type", usageType);
        cmd.Parameters.AddWithValue("qty", quantity);
        cmd.Parameters.AddWithValue("key", idempotencyKey);

        var rows = cmd.ExecuteNonQuery();
        return rows > 0;
    }


    public Dictionary<string, long> GetMonthlyUsage(int tenantId)
    {
        using var conn = new NpgsqlConnection(_connStr);
        conn.Open();

        using var cmd = new NpgsqlCommand("""
            SELECT usage_type, COALESCE(SUM(quantity), 0)
            FROM usage_events
            WHERE tenant_id = @tid
              AND created_at >= date_trunc('month', NOW())
            GROUP BY usage_type
            """, conn);

        cmd.Parameters.AddWithValue("tid", tenantId);

        // Dictionary: "api_call" → 150, "token" → 45000
        var result = new Dictionary<string, long>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            result[reader.GetString(0)] = reader.GetInt64(1);
        }

        return result;
    }
}