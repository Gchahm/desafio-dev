using DesafioDev.Application.Common.Interfaces.Repositories;
using DesafioDev.Domain.Entities;
using DesafioDev.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DesafioDev.Infrastructure.Repositories;

public class StoreRepository(ApplicationDbContext context) : IStoreRepository
{
    public async Task<List<Store>> GetAllWithTransactionsAsync(CancellationToken cancellationToken)
    {
        return await context.Stores
            .Include(s => s.Transactions)
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Store?> GetByNameAsync(string name, CancellationToken cancellationToken)
    {
        return await context.Stores.FirstOrDefaultAsync(s => s.Name == name, cancellationToken);
    }

    public async Task AddAsync(Store store, CancellationToken cancellationToken)
    {
        await context.Stores.AddAsync(store, cancellationToken);
    }
}
