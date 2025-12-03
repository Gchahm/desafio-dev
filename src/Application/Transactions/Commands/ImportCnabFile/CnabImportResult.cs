namespace DesafioDev.Application.Transactions.Commands.ImportCnabFile;

/// <summary>
/// Result of a CNAB file import operation
/// </summary>
public record CnabImportResult
{
    /// <summary>
    /// Total number of lines in the file
    /// </summary>
    public int TotalLines { get; init; }

    /// <summary>
    /// Number of transactions successfully imported
    /// </summary>
    public int ValidLines { get; init; }

    /// <summary>
    /// Number of transactions that failed to import
    /// </summary>
    public int InvalidLines { get; init; }

    /// <summary>
    /// List of error messages for failed imports
    /// </summary>
    public List<string> Errors { get; init; } = new();

    /// <summary>
    /// Indicates if the import was completely successful
    /// </summary>
    public bool IsSuccess { get; init; }
}