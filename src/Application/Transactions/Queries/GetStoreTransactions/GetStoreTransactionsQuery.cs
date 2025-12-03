namespace DesafioDev.Application.Transactions.Queries.GetStoreTransactions;

/// <summary>
/// Query to get all stores with their transactions and balances
/// </summary>
public record GetStoreTransactionsQuery : IRequest<List<StoreDto>>;
