namespace BillingApi.Models;

// One tenant's monthly report data.
public sealed record ReportData(
    int TenantId, string TenantEmail, string PlanName, string Period,
    long ApiCallsUsed, long ApiCallLimit,
    long TokensUsed, long TokenLimit,
    decimal Cost);