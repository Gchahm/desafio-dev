namespace DesafioDev.Application.Common.Models;

/// <summary>
/// DTO representing parsed data from a single CNAB line
/// Contains primitive types before domain validation
/// </summary>
public record CnabLineData
{
    /// <summary>
    /// Transaction type (1-9)
    /// </summary>
    public int LineNumber { get; init; }
    /// <summary>
    /// Transaction type (1-9)
    /// </summary>
    public int Type { get; init; }

    /// <summary>
    /// Date in YYYYMMDD format
    /// </summary>
    public string Date { get; init; } = string.Empty;

    /// <summary>
    /// Transaction amount in cents (to be divided by 100 for decimal representation)
    /// </summary>
    public long ValueInCents { get; init; }

    /// <summary>
    /// Beneficiary's CPF (11 digits)
    /// </summary>
    public string Cpf { get; init; } = string.Empty;

    /// <summary>
    /// Card number used in the transaction (12 digits)
    /// </summary>
    public string CardNumber { get; init; } = string.Empty;

    /// <summary>
    /// Time in HHMMSS format
    /// </summary>
    public string Time { get; init; } = string.Empty;

    /// <summary>
    /// Store representative's name
    /// </summary>
    public string StoreOwner { get; init; } = string.Empty;

    /// <summary>
    /// Store name
    /// </summary>
    public string StoreName { get; init; } = string.Empty;
}
