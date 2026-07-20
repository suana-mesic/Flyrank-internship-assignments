using ItemsApi.Models;
using Npgsql;
namespace ItemsApi.Repositories
{
    public class PostgresItemRepository : IItemRepository
    {
        private readonly NpgsqlDataSource _dataSource;

        public PostgresItemRepository(NpgsqlDataSource dataSource) => _dataSource = dataSource;

        public async Task<IReadOnlyList<Item>> GetAllAsync(CancellationToken ct)
        {
            const string sql = """
                SELECT id, name, created_at_utc
                FROM items
                ORDER BY created_at_utc
                """;

            await using var cmd = _dataSource.CreateCommand(sql);
            await using var reader = await cmd.ExecuteReaderAsync(ct);

            var items = new List<Item>();
            while (await reader.ReadAsync(ct))
            {
                items.Add(new Item(
                    reader.GetGuid(0),
                    reader.GetString(1),
                    reader.GetDateTime(2)));
            }

            return items;
        }

        public async Task<Item?> GetByIdAsync(Guid id, CancellationToken ct)
        {
            const string sql = "SELECT id, name, created_at_utc FROM items WHERE id = $1;";
            await using var cmd = _dataSource.CreateCommand(sql);
            cmd.Parameters.AddWithValue(id);

            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (!await reader.ReadAsync(ct))
                return null;

            return new Item(reader.GetGuid(0), reader.GetString(1), reader.GetDateTime(2));
        }

        public async Task<Item> AddAsync(string name, CancellationToken ct)
        {
            const string sql = "INSERT INTO items (id, name, created_at_utc) VALUES ($1,$2,$3);";
            var item = new Item(Guid.NewGuid(), name, DateTime.UtcNow);

            await using var cmd=_dataSource.CreateCommand(sql);
            cmd.Parameters.AddWithValue(item.Id);
            cmd.Parameters.AddWithValue(item.Name);
            cmd.Parameters.AddWithValue(item.CreatedAtUtc);

            await cmd.ExecuteNonQueryAsync(ct);
            return item;
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct)
        {
            const string sql = "DELETE FROM items WHERE id = $1;";

            await using var cmd = _dataSource.CreateCommand(sql);
            cmd.Parameters.AddWithValue(id);

            var affected = await cmd.ExecuteNonQueryAsync(ct);
            return affected > 0;
        }

    }
}
