namespace BackgroundJobApi.Services
{
    public interface IAlertService
    {
        Task RaiseAsync(string subject, string details, CancellationToken ct = default);
    }

    // Minimal alert sink: a job failure is surfaced as a clearly-marked, high-severity
    // ALERT line rather than being buried among info logs, so someone actually finds out.
    // A production version would send this to email / Slack / a webhook — the interface
    // stays the same, only the implementation changes.
    public class ConsoleAlertService : IAlertService
    {
        private readonly ILogger<ConsoleAlertService> _logger;

        public ConsoleAlertService(ILogger<ConsoleAlertService> logger) => _logger = logger;

        public Task RaiseAsync(string subject, string details, CancellationToken ct = default)
        {
            _logger.LogCritical("ALERT: {Subject} - {Details}", subject, details);
            return Task.CompletedTask;
        }
    }
}
