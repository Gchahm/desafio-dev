using DesafioDev.Domain.Common;
using DesafioDev.Domain.Enums;
using DesafioDev.Domain.ValueObjects;

namespace DesafioDev.Domain.Entities;

/// <summary>
/// Represents a financial transaction from CNAB file
/// </summary>
public class FinancialTransaction : BaseEntity
{
    /// <summary>
    /// Type of transaction (1-9)
    /// </summary>
    public TransactionType Type { get; set; }

    /// <summary>
    /// Date of the transaction
    /// </summary>
    public DateOnly Date { get; set; }

    /// <summary>
    /// Time of the transaction (UTC-3)
    /// </summary>
    public TimeOnly Time { get; set; }

    /// <summary>
    /// Transaction amount (normalized from CNAB value / 100)
    /// </summary>
    public Money Amount { get; set; } = Money.FromDecimal(0);

    /// <summary>
    /// Beneficiary's CPF
    /// </summary>
    public CPF Cpf { get; set; } = CPF.CreateUnchecked("00000000000");

    /// <summary>
    /// Card number used in the transaction
    /// </summary>
    public CardNumber Card { get; set; } = CardNumber.Create("000000000000");

    /// <summary>
    /// Nature of the transaction (Income or Expense)
    /// </summary>
    public TransactionNature Nature => Type switch
    {
        TransactionType.Debit => TransactionNature.Income,
        TransactionType.Boleto or TransactionType.Financing => TransactionNature.Expense,
        TransactionType.Credit => TransactionNature.Income,
        TransactionType.LoanReceipt => TransactionNature.Income,
        TransactionType.Sales => TransactionNature.Income,
        TransactionType.TedReceipt => TransactionNature.Income,
        TransactionType.DocReceipt => TransactionNature.Income,
        TransactionType.Rent => TransactionNature.Expense,
        _ => throw new ArgumentException($"Invalid transaction type: {Type}", nameof(Type))
    };

    /// <summary>
    /// Description of the transaction type
    /// </summary>
    public string Description => Type switch
    {
        TransactionType.Debit => "Debit",
        TransactionType.Boleto => "Boleto",
        TransactionType.Financing => "Financing",
        TransactionType.Credit => "Credit",
        TransactionType.LoanReceipt => "Loan Receipt",
        TransactionType.Sales => "Sales",
        TransactionType.TedReceipt => "TED Receipt",
        TransactionType.DocReceipt => "DOC Receipt",
        TransactionType.Rent => "Rent",
        _ => throw new ArgumentException($"Invalid transaction type: {Type}", nameof(Type))
    };

    /// <summary>
    /// Foreign key to the Store
    /// </summary>
    public int StoreId { get; set; }

    /// <summary>
    /// Navigation property to the Store
    /// </summary>
    public virtual Store Store { get; set; } = null!;

    /// <summary>
    /// Date and time when the transaction was created in the system
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets the signed amount (positive for income, negative for expense)
    /// </summary>
    public decimal GetSignedAmount()
    {
        return Nature == TransactionNature.Income
            ? Amount.ToDecimal()
            : -Amount.ToDecimal();
    }
}