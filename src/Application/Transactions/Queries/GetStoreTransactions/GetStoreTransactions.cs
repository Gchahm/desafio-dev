using DesafioDev.Application.Common.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DesafioDev.Application.Transactions.Queries.GetStoreTransactions;

/// <summary>
/// Query to get all stores with their transactions and balances
/// </summary>
public record GetStoreTransactionsQuery : IRequest<List<StoreTransactionsDto>>;

/// <summary>
/// Handler for GetStoreTransactionsQuery
/// </summary>
public class GetStoreTransactionsQueryHandler(IStoreRepository stores)
    : IRequestHandler<GetStoreTransactionsQuery, List<StoreTransactionsDto>>
{
    public async Task<List<StoreTransactionsDto>> Handle(
        GetStoreTransactionsQuery request,
        CancellationToken cancellationToken)
    {
        // Get all stores with their transactions
        var storeEntities = await stores.GetAllWithTransactionsAsync(cancellationToken);

        // Map to DTOs
        var result = storeEntities.Select(store => new StoreTransactionsDto
        {
            Id = store.Id,
            Name = store.Name,
            OwnerName = store.OwnerName,
            Transactions = store.Transactions
                .OrderByDescending(t => t.Date)
                .ThenByDescending(t => t.Time)
                .Select(t => new TransactionDto
                {
                    Id = t.Id,
                    Description = t.Description,
                    Nature = t.Nature.ToString(),
                    Date = t.Date.ToDateTime(t.Time),
                    SignedAmount = t.GetSignedAmount(),
                    Cpf = t.Cpf.ToFormattedString(),
                    CardNumber = t.Card.ToMaskedString(),
                    CreatedAt = t.CreatedAt
                })
                .ToList(),
            TotalBalance = store.Transactions.Sum(t => t.GetSignedAmount())
        }).ToList();

        return result;
    }
}