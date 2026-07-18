using BackgroundJobApi.Models;
using BackgroundJobApi.Store;
using System.Threading.Channels;

namespace BackgroundJobApi.Services
{
    public class JobWorker : BackgroundService
    {
        private readonly Channel<Guid> _channel;
        private readonly IJobStore _store;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<JobWorker> _logger;

        public JobWorker(
              Channel<Guid> channel,
              IJobStore store,
              IServiceProvider serviceProvider,
              ILogger<JobWorker> logger)
        {
            _channel = channel;
            _store = store;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var jobId in _channel.Reader.ReadAllAsync(stoppingToken))
            {
                var job = _store.GetById(jobId);

                if (job is null || job.Status != JobStatus.Queued)
                    continue;
                job.Status = JobStatus.Processing;

                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var ai = scope.ServiceProvider.GetRequiredService<AiService>();

                    var maxRetries = 3;
                    string? result = null;
                    for (var attempt = 1; attempt <= maxRetries; attempt++)
                    {

                        try
                        {
                            result = await ai.SummarizeAsync(job.InputText, stoppingToken);
                            break;
                        }
                        catch (Exception ex) when (attempt < maxRetries)
                        {
                            _logger.LogWarning(ex, "Job {Id} attempt {Attempt} failed, retrying",
                          job.Id, attempt);
                            await Task.Delay(1000 * attempt, stoppingToken);
                        }
                    }

                    job.Result= result;
                    job.Status = JobStatus.Completed;
                    job.CompletedAtUtc = DateTime.UtcNow;

                    _logger.LogInformation("Job {Id} completed", job.Id);

                }
                catch (Exception ex)
                {
                    job.Error = ex.Message;
                    job.Status = JobStatus.Failed;
                    job.CompletedAtUtc = DateTime.UtcNow;

                    _logger.LogError(ex, "Job {Id} failed", job.Id);
                }
            }
        }
    }
}
