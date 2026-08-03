using BackgroundJobApi.Models;
using System.Collections.Concurrent;

namespace BackgroundJobApi.Store
{
    public class InMemoryJobStore : IJobStore
    {
        private readonly ConcurrentDictionary<Guid, BackgroundJob> _jobs = new();
        private readonly ConcurrentDictionary<string, Guid> _byKey = new();

        public (BackgroundJob job, bool isNew) AddOrGet(BackgroundJob job)
        {
            // No idempotency key → just store it.
            if (string.IsNullOrWhiteSpace(job.IdempotencyKey))
            {
                _jobs[job.Id] = job;
                return (job, true);
            }

            // Store first, so the job is retrievable the instant the key points at it.
            _jobs[job.Id] = job;

            // GetOrAdd is atomic: concurrent callers with the same key all see one winner.
            var winningId = _byKey.GetOrAdd(job.IdempotencyKey, job.Id);
            if (winningId == job.Id)
                return (job, true);

            // Someone else won the key — discard ours and return theirs.
            _jobs.TryRemove(job.Id, out _);
            return (_jobs[winningId], false);
        }

        public BackgroundJob? GetById(Guid id) => _jobs.TryGetValue(id, out var job) ? job : null;
    }
}
