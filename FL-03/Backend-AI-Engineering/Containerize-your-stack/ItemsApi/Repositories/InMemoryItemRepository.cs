using ItemsApi.Models;
using System.Collections.Concurrent;
namespace ItemsApi.Repositories
{
    public class InMemoryItemRepository : IItemRepository
    {
        private readonly ConcurrentDictionary<Guid, Item> _items = new();
        public Task<IReadOnlyList<Item>> GetAllAsync(CancellationToken ct)
        {
            IReadOnlyList<Item> result = _items.Values.OrderBy(x=>x.CreatedAtUtc).ToList();
            return Task.FromResult(result);
        }

        public Task<Item?> GetByIdAsync(Guid id, CancellationToken ct)
        {
            _items.TryGetValue(id, out var item);
            return Task.FromResult(item);
        }

        public Task<Item> AddAsync(string name, CancellationToken ct)
        {
            var item = new Item(Guid.NewGuid(), name, DateTime.UtcNow);
            _items[item.Id] = item;
            return Task.FromResult(item);
        }

        public Task<bool> DeleteAsync(Guid id, CancellationToken ct)
           => Task.FromResult(_items.Remove(id, out _));
    }
}
