using ItemsApi.Models;

namespace ItemsApi.Repositories
{
    public interface IItemRepository
    {
        Task<IReadOnlyList<Item>> GetAllAsync(CancellationToken ct);
        Task<Item?> GetByIdAsync(Guid id, CancellationToken ct);
        Task<Item> AddAsync(string name, CancellationToken ct);
        Task<bool> DeleteAsync(Guid id, CancellationToken ct);
    }
}
