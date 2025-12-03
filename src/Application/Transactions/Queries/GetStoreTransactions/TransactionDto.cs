namespace DesafioDev.Application.Transactions.Queries.GetStoreTransactions;

/// <summary>
/// DTO representing a single transaction
/// </summary>
public class TransactionDto
{
    /// <summary>
    /// Transaction ID
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Type description (e.g., "Debit", "Credit")
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Transaction nature (Income or Expense)
    /// </summary>
    public string Nature { get; init; } = string.Empty;

    /// <summary>
    /// Transaction date
    /// </summary>
    public DateTime Date { get; init; }

    /// <summary>
    /// Transaction time
    /// </summary>
    public TimeOnly Time { get; init; }

    /// <summary>
    /// Signed amount (positive for income, negative for expense)
    /// </summary>
    public decimal SignedAmount { get; init; }

    /// <summary>
    /// Beneficiary's CPF (formatted)
    /// </summary>
    public string Cpf { get; init; } = string.Empty;

    /// <summary>
    /// Card number (masked)
    /// </summary>
    public string CardNumber { get; init; } = string.Empty;

    /// <summary>
    /// Date and time when imported
    /// </summary>
    public DateTime CreatedAt { get; init; }
}