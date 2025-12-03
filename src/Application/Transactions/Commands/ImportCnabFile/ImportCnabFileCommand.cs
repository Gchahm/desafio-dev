using DesafioDev.Application.Common.Models;

namespace DesafioDev.Application.Transactions.Commands.ImportCnabFile;

/// <summary>
/// Command to import a CNAB file
/// </summary>
public record ImportCnabFileCommand : IRequest<CnabImportResult>
{
    /// <summary>
    /// The CNAB file stream to import
    /// </summary>
    public Stream FileStream { get; init; } = null!;

    /// <summary>
    /// Original filename (for logging/tracking purposes)
    /// </summary>
    public string FileName { get; init; } = string.Empty;

    public bool IgnoreErrors { get; set; } = false;
}