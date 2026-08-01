
using BillingApi.Repositories;

namespace BillingApi.Services
{
    public sealed class ReportWorker : BackgroundService
    {
        private readonly ReportRepository _reports;
        private readonly ReportService _pdf;
        private readonly PricingService _pricing;
        private readonly string _reportsPath;

        public ReportWorker(ReportRepository reports, ReportService pdf, PricingService pricing, IConfiguration config)
        {
            _reports = reports;
            _pdf = pdf;
            _pricing = pricing;
            _reportsPath = config["Reports:Path"] ?? Path.Combine(AppContext.BaseDirectory, "reports");
            Directory.CreateDirectory(_reportsPath);
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var job = _reports.ClaimNextPending();
                if (job is null) { await Task.Delay(2000, stoppingToken); continue; }

                try
                {
                    var data = _reports.GetReportData(job.Value.tenantId, job.Value.period);
                    if (data is null) { _reports.MarkFailed(job.Value.id, "No data for tenant/period"); continue; }

                    var cost = _pricing.CalculateCost(data.ApiCallsUsed, 0, 0, data.TokensUsed);
                    var pdf = _pdf.GeneratePdf(data with { Cost = cost });

                    var path = Path.Combine(_reportsPath, $"report-{job.Value.id}.pdf");
                    await File.WriteAllBytesAsync(path, pdf, stoppingToken);

                    _reports.MarkDone(job.Value.id, path);
                    Console.WriteLine($"[report] done #{job.Value.id} -> {path}");
                }
                catch (Exception ex)
                {
                    _reports.MarkFailed(job.Value.id, ex.Message);
                    Console.WriteLine($"[report] FAIL #{job.Value.id}: {ex.Message}");
                }
            }
        }
    }
}
