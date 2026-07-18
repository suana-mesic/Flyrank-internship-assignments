using BackgroundJobApi.Models;

namespace BackgroundJobApi.Store
{
    public interface IJobStore
    {
        void Add(BackgroundJob job);
        BackgroundJob? GetById(Guid id);
        BackgroundJob? GetByIdempotencyKey(string key);
    }
}
