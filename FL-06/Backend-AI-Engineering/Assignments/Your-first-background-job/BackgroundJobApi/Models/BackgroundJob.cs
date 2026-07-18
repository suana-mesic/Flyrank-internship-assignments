namespace BackgroundJobApi.Models
{
    public enum JobStatus
    {
        Queued,
        Processing,
        Completed,
        Failed
    }
    public class BackgroundJob
    {
        public Guid Id { get; set; }
        public string InputText { get; set; } = string.Empty;
        public string? Result { get; set; }
        public string? Error { get; set; }
        public JobStatus Status { get; set; } = JobStatus.Queued;
        public string? IdempotencyKey { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAtUtc { get; set; }
    }
}
