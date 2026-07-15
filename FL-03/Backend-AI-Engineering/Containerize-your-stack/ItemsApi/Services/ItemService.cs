using ItemsApi.Models;
using ItemsApi.Repositories;

namespace ItemsApi.Services
{
    public class ItemService
    {
        private readonly IItemRepository _repository;
        public ItemService(IItemRepository repository) => _repository = repository;

        public Task<IReadOnlyList<Item>> GetAllAsync(CancellationToken ct) => _repository.GetAllAsync(ct);
        public Task<Item?> GetByIdAsync(Guid id, CancellationToken ct)
        => _repository.GetByIdAsync(id, ct);

        public Task<Item> CreateAsync(string name, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name must not be empty", nameof(name));

            return _repository.AddAsync(name.Trim(), ct);
        }
        public Task<bool> DeleteAsync(Guid id, CancellationToken ct)
       => _repository.DeleteAsync(id, ct);
    }
}
