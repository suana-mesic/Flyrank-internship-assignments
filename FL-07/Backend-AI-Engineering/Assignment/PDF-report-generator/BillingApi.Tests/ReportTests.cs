using System.Text;
using BillingApi.Models;
using BillingApi.Repositories;
using BillingApi.Services;
using Npgsql;

namespace BillingApi.Tests;

public class ReportTests
{
    private const string ConnStr =
        "Host=localhost;Port=5434;Username=billing_user;Password=billing_pass;Database=billingdb";

    [Fact]
    public void GeneratePdf_ProducesValidPdfBytes()
    {
        // Pure: rendering a report yields non-empty bytes that start with the PDF magic header.
        var data = new ReportData(1, "test@billing.com", "Pro", "2026-07",
            ApiCallsUsed: 2, ApiCallLimit: 10000, TokensUsed: 30, TokenLimit: 1000000, Cost: 0.00218m);

        var bytes = new ReportService().GeneratePdf(data);

        Assert.True(bytes.Length > 0);
        Assert.Equal("%PDF", Encoding.ASCII.GetString(bytes, 0, 4)); // valid PDF signature
    }

    [Fact]
    public void GetReportData_AggregatesTenantUsage()
    {
        // Integration: aggregation returns a row with the tenant and plan filled in.
        var repo = new ReportRepository(ConnStr);
        var data = repo.GetReportData(1, "2026-07");

        Assert.NotNull(data);
        Assert.Equal(1, data!.TenantId);
        Assert.False(string.IsNullOrWhiteSpace(data.PlanName));
        Assert.True(data.ApiCallsUsed >= 0 && data.TokensUsed >= 0);
    }

    [Fact]
    public void JobLifecycle_PendingBecomesDone()
    {
        // Integration: a queued job starts pending and can be marked done.
        var repo = new ReportRepository(ConnStr);
        var id = repo.CreatePending(1, "2099-01");   // unique future period, won't clash
        try
        {
            var created = repo.GetById(id);
            Assert.Equal("pending", created!.Value.status);

            repo.MarkDone(id, "C:/tmp/fake-report.pdf");
            var done = repo.GetById(id);
            Assert.Equal("done", done!.Value.status);
            Assert.Equal("C:/tmp/fake-report.pdf", done.Value.filePath);
        }
        finally
        {
            Cleanup(id);
        }
    }

    private static void Cleanup(int id)
    {
        using var conn = new NpgsqlConnection(ConnStr);
        conn.Open();
        using var cmd = new NpgsqlCommand("DELETE FROM reports WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("id", id);
        cmd.ExecuteNonQuery();
    }
}