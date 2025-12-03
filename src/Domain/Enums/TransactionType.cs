namespace DesafioDev.Domain.Enums;

/// <summary>
/// Represents the type of financial transaction
/// </summary>
public enum TransactionType
{
    /// <summary>
    /// Debit - Income (+)
    /// </summary>
    Debit = 1,

    /// <summary>
    /// Boleto - Expense (-)
    /// </summary>
    Boleto = 2,

    /// <summary>
    /// Financing - Expense (-)
    /// </summary>
    Financing = 3,

    /// <summary>
    /// Credit - Income (+)
    /// </summary>
    Credit = 4,

    /// <summary>
    /// Loan Receipt - Income (+)
    /// </summary>
    LoanReceipt = 5,

    /// <summary>
    /// Sales - Income (+)
    /// </summary>
    Sales = 6,

    /// <summary>
    /// TED Receipt - Income (+)
    /// </summary>
    TedReceipt = 7,

    /// <summary>
    /// DOC Receipt - Income (+)
    /// </summary>
    DocReceipt = 8,

    /// <summary>
    /// Rent - Expense (-)
    /// </summary>
    Rent = 9
}
