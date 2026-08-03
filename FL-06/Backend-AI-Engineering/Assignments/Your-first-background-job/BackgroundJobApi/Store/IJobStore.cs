using BackgroundJobApi.Models;

namespace BackgroundJobApi.Store
{
    public interface IJobStore
    {
        // Atomically stores the job, or — if its idempotency key is already taken —
        // returns the job that owns that key. isNew tells the caller which happened.
        (BackgroundJob job, bool isNew) AddOrGet(BackgroundJob job);

        BackgroundJob? GetById(Guid id);
    }
}
