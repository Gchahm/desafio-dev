using DesafioDev.Domain.Entities;

namespace DesafioDev.Application.Common.Interfaces.Repositories;

public interface IStoreRepository
{
    Task<List<Store>> GetAllWithTransactionsAsync(CancellationToken cancellationToken);

    Task<Store?> GetByNameAsync(string name, CancellationToken cancellationToken);

    Task<Store?> GetByNameAndOwnerAsync(string name, string owner, CancellationToken cancellationToken);

    Task AddAsync(Store store, CancellationToken cancellationToken);
}
