namespace DesafioDev.Application.Transactions.Queries.GetStoreTransactions;

/// <summary>
/// DTO representing a store with its transactions and balance
/// </summary>
public class StoreTransactionsDto
{
    /// <summary>
    /// Store ID
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Store name
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Store owner name
    /// </summary>
    public string OwnerName { get; init; } = string.Empty;

    /// <summary>
    /// List of transactions for this store
    /// </summary>
    public List<TransactionDto> Transactions { get; init; } = new();

    /// <summary>
    /// Total balance (sum of all transactions: income - expense)
    /// </summary>
    public decimal TotalBalance { get; init; }
}


