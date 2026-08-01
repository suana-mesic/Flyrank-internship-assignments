using BillingApi.Models;
using Npgsql;

namespace BillingApi.Repositories;

public class ReportRepository
{
    private readonly string _connStr;
    public ReportRepository(string connStr) => _connStr = connStr;

    // Aggregates one tenant's usage for a given month ("2026-07").
    // Cost is left at 0 here; it's computed by PricingService when the report is built.
    public ReportData? GetReportData(int tenantId, string period)
    {
        using var conn = new NpgsqlConnection(_connStr);
        conn.Open();
        using var cmd = new NpgsqlCommand("""
            SELECT t.id, t.email, p.name, p.api_call_limit, p.token_limit,
                   COALESCE(SUM(CASE WHEN ue.usage_type = 'api_call' THEN ue.quantity END), 0) AS calls,
                   COALESCE(SUM(CASE WHEN ue.usage_type = 'token'    THEN ue.quantity END), 0) AS tokens
            FROM tenants t
            JOIN plans p ON p.id = t.plan_id
            LEFT JOIN usage_events ue ON ue.tenant_id = t.id
                 AND ue.created_at >= to_date(@period || '-01', 'YYYY-MM-DD')
                 AND ue.created_at <  (to_date(@period || '-01', 'YYYY-MM-DD') + interval '1 month')
            WHERE t.id = @tid
            GROUP BY t.id, t.email, p.name, p.api_call_limit, p.token_limit
            """, conn);
        cmd.Parameters.AddWithValue("tid", tenantId);
        cmd.Parameters.AddWithValue("period", period);

        using var reader = cmd.ExecuteReader();
        if (!reader.Read()) return null;

        return new ReportData(
            reader.GetInt32(0), reader.GetString(1), reader.GetString(2), period,
            reader.GetInt64(5), reader.GetInt32(3),   // calls, apiCallLimit
            reader.GetInt64(6), reader.GetInt32(4),   // tokens, tokenLimit
            Cost: 0m);
    }

    // Creates a queued report job; returns its id.
    public int CreatePending(int tenantId, string period)
    {
        using var conn = new NpgsqlConnection(_connStr);
        conn.Open();
        using var cmd = new NpgsqlCommand(
            "INSERT INTO reports (tenant_id, period, status) VALUES (@t, @p, 'pending') RETURNING id", conn);
        cmd.Parameters.AddWithValue("t", tenantId);
        cmd.Parameters.AddWithValue("p", period);
        return (int)cmd.ExecuteScalar()!;
    }

    // Report status row.
    public (int id, int tenantId, string period, string status, string? filePath, string? error)? GetById(int id)
    {
        using var conn = new NpgsqlConnection(_connStr);
        conn.Open();
        using var cmd = new NpgsqlCommand(
            "SELECT id, tenant_id, period, status, file_path, error FROM reports WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("id", id);
        using var r = cmd.ExecuteReader();
        if (!r.Read()) return null;
        return (r.GetInt32(0), r.GetInt32(1), r.GetString(2), r.GetString(3),
                r.IsDBNull(4) ? null : r.GetString(4), r.IsDBNull(5) ? null : r.GetString(5));
    }

    // Atomically claims the oldest pending job (marks it processing). Null if none.
    public (int id, int tenantId, string period)? ClaimNextPending()
    {
        using var conn = new NpgsqlConnection(_connStr);
        conn.Open();
        using var cmd = new NpgsqlCommand("""
            UPDATE reports SET status = 'processing', updated_at = now()
            WHERE id = (SELECT id FROM reports WHERE status = 'pending'
                        ORDER BY id LIMIT 1 FOR UPDATE SKIP LOCKED)
            RETURNING id, tenant_id, period
            """, conn);
        using var r = cmd.ExecuteReader();
        if (!r.Read()) return null;
        return (r.GetInt32(0), r.GetInt32(1), r.GetString(2));
    }

    public void MarkDone(int id, string filePath)
    {
        using var conn = new NpgsqlConnection(_connStr);
        conn.Open();
        using var cmd = new NpgsqlCommand(
            "UPDATE reports SET status = 'done', file_path = @f, updated_at = now() WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("f", filePath);
        cmd.Parameters.AddWithValue("id", id);
        cmd.ExecuteNonQuery();
    }

    public void MarkFailed(int id, string error)
    {
        using var conn = new NpgsqlConnection(_connStr);
        conn.Open();
        using var cmd = new NpgsqlCommand(
            "UPDATE reports SET status = 'failed', error = @e, updated_at = now() WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("e", error);
        cmd.Parameters.AddWithValue("id", id);
        cmd.ExecuteNonQuery();
    }
}