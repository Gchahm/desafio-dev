namespace DesafioDev.Domain.Entities;

/// <summary>
/// Represents a store with its financial transactions
/// </summary>
public class Store : BaseAuditableEntity
{
    /// <summary>
    /// Store name (max 19 characters from CNAB)
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Store owner's name (max 14 characters from CNAB)
    /// </summary>
    public string OwnerName { get; set; } = string.Empty;

    /// <summary>
    /// Collection of financial transactions for this store
    /// </summary>
    public virtual ICollection<FinancialTransaction> Transactions { get; set; } = new List<FinancialTransaction>();
}
