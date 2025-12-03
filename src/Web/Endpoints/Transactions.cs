using DesafioDev.Application.Common.Models;
using DesafioDev.Application.Transactions.Commands.ImportCnabFile;
using DesafioDev.Application.Transactions.Queries.GetStoreTransactions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace DesafioDev.Web.Endpoints;

/// <summary>
/// Endpoints for financial transaction operations
/// </summary>
public class Transactions : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetStoreTransactions);
        groupBuilder.MapPost(ImportCnabFile, "import")
            .DisableAntiforgery()
            .Accepts<IFormFile>("multipart/form-data");
    }

    /// <summary>
    /// Get all stores with their transactions and balances
    /// </summary>
    /// <param name="sender">MediatR sender</param>
    /// <returns>List of stores with transactions and total balance</returns>
    public async Task<Ok<List<StoreDto>>> GetStoreTransactions(ISender sender)
    {
        var result = await sender.Send(new GetStoreTransactionsQuery());
        return TypedResults.Ok(result);
    }

    /// <summary>
    /// Import a CNAB file
    /// </summary>
    /// <param name="sender">MediatR sender</param>
    /// <param name="file">The CNAB file to import</param>
    /// <returns>Import result with statistics</returns>
    public async Task<Results<Ok<CnabImportResult>, BadRequest<string>>> ImportCnabFile(
        ISender sender,
        IFormFile file)
    {
        // Validate file
        if (file == null || file.Length == 0)
        {
            return TypedResults.BadRequest("No file uploaded");
        }

        // Validate file extension (optional - CNAB files might have .txt or no extension)
        var allowedExtensions = new[] { ".txt", ".cnab", "" };
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(fileExtension) && !string.IsNullOrEmpty(fileExtension))
        {
            return TypedResults.BadRequest($"Invalid file type. Allowed types: {string.Join(", ", allowedExtensions.Where(e => !string.IsNullOrEmpty(e)))}");
        }

        // Validate file size (max 10MB for safety)
        const long maxFileSize = 10 * 1024 * 1024; // 10MB
        if (file.Length > maxFileSize)
        {
            return TypedResults.BadRequest($"File size exceeds maximum allowed size of {maxFileSize / (1024 * 1024)}MB");
        }

        try
        {
            // Open file stream and send command
            using var stream = file.OpenReadStream();

            var command = new ImportCnabFileCommand
            {
                FileStream = stream,
                FileName = file.FileName
            };

            var result = await sender.Send(command);

            return TypedResults.Ok(result);
        }
        catch (Exception ex)
        {
            return TypedResults.BadRequest($"Error processing file: {ex.Message}");
        }
    }
}
