using DesafioDev.Application.Common.Interfaces.Repositories;

namespace DesafioDev.Application.Transactions.Queries.GetStoreTransactions;


/// <summary>
/// Handler for GetStoreTransactionsQuery
/// </summary>
public class GetStoreTransactionsQueryHandler(IStoreRepository stores)
    : IRequestHandler<GetStoreTransactionsQuery, List<StoreDto>>
{
    public async Task<List<StoreDto>> Handle(
        GetStoreTransactionsQuery request,
        CancellationToken cancellationToken)
    {
        // Get all stores with their transactions
        var storeEntities = await stores.GetAllWithTransactionsAsync(cancellationToken);

        // Map to DTOs
        return storeEntities.Select(StoreDto.FromEntity).ToList();
    }
}