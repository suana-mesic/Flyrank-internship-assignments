using BackgroundJobApi.Models;
using System.Collections.Concurrent;

namespace BackgroundJobApi.Store
{
    public class InMemoryJobStore : IJobStore
    {
        private readonly ConcurrentDictionary<Guid, BackgroundJob> _jobs = new();
        public void Add(BackgroundJob job) => _jobs[job.Id] = job;
        public BackgroundJob? GetById(Guid id) => _jobs.TryGetValue(id, out var job) ? job : null;
        public BackgroundJob? GetByIdempotencyKey(string key) => _jobs.Values.FirstOrDefault(j => j.IdempotencyKey == key);
    }
}
