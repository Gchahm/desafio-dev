using DesafioDev.Application.Common.Interfaces;
using DesafioDev.Application.Common.Interfaces.Repositories;
using DesafioDev.Application.Common.Models;
using DesafioDev.Domain.Entities;
using DesafioDev.Domain.Enums;
using DesafioDev.Domain.ValueObjects;

namespace DesafioDev.Application.Transactions.Commands.ImportCnabFile;

/// <summary>
/// Handler for ImportCnabFileCommand
/// Orchestrates the import process: parse file, create/update stores, save transactions
/// </summary>
public class ImportCnabFileCommandHandler(
    IStoreRepository stores,
    IFinancialTransactionRepository transactions,
    IUnitOfWork uow,
    ICnabFileParser parser) : IRequestHandler<ImportCnabFileCommand, CnabImportResult>
{
    public async Task<CnabImportResult> Handle(
        ImportCnabFileCommand request,
        CancellationToken cancellationToken)
    {
        var result = new CnabImportResult();
        var errors = new List<string>();

        try
        {
            // Parse the file
            var parsedLines = parser.ParseFile(request.FileStream).ToList();
            result = result with { TotalLines = parsedLines.Count };

            if (parsedLines.Count == 0)
            {
                return result with
                {
                    Errors = new List<string> { "File is empty or contains no valid data" }
                };
            }

            // Group transactions by store
            var transactionsByStore = parsedLines.GroupBy(line => line.StoreName.Trim());

            foreach (var storeGroup in transactionsByStore)
            {
                var storeName = storeGroup.Key;

                try
                {
                    // Get the store owner from the first transaction
                    var firstTransaction = storeGroup.First();
                    var storeOwner = firstTransaction.StoreOwner.Trim();

                    // Find or create store
                    var store = await GetOrCreateStoreAsync(
                        storeName,
                        storeOwner,
                        cancellationToken);

                    // Process each transaction for this store
                    foreach (var lineData in storeGroup)
                    {
                        try
                        {
                            var transaction = CreateTransaction(lineData, store);
                            await transactions.AddAsync(transaction, cancellationToken);
                            result = result with { ValidLines = result.ValidLines + 1 };
                        }
                        catch (Exception ex)
                        {
                            errors.Add($"Line '{lineData.LineNumber + 1}': Failed to create transaction - {ex.Message}");
                            result = result with { InvalidLines = result.InvalidLines + 1 };
                        }
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"Store '{storeName}': {ex.Message}");
                    result = result with
                    {
                        InvalidLines = result.InvalidLines + storeGroup.Count()
                    };
                }
            }

            result = result with
            {
                Errors = errors
            };

            // Save all changes to database
            if (result.ValidLines > 0 && (result.InvalidLines == 0 || request.IgnoreErrors))
            {
                await uow.SaveChangesAsync(cancellationToken);
                result = result with { IsSuccess = true };
            }

            return result;
        }
        catch (Exception ex)
        {
            errors.Add($"Critical error during import: {ex.Message}");
            return result with
            {
                Errors = errors,
                InvalidLines = result.TotalLines - result.ValidLines
            };
        }
    }

    /// <summary>
    /// Finds an existing store or creates a new one
    /// </summary>
    private async Task<Store> GetOrCreateStoreAsync(
        string storeName,
        string storeOwner,
        CancellationToken cancellationToken)
    {
        // Try to find existing store by name
        var store = await stores.GetByNameAsync(storeName, cancellationToken);

        if (store != null) return store;

        // Create new store
        store = new Store
        {
            Name = storeName,
            OwnerName = storeOwner
        };

        await stores.AddAsync(store, cancellationToken);

        return store;
    }

    /// <summary>
    /// Creates a FinancialTransaction entity from parsed CNAB line data
    /// </summary>
    private static FinancialTransaction CreateTransaction(
        CnabLineData lineData,
        Store store)
    {
        // Parse and validate transaction type
        if (!Enum.IsDefined(typeof(TransactionType), lineData.Type))
        {
            throw new ArgumentException($"Invalid transaction type: {lineData.Type}");
        }

        var transactionType = (TransactionType)lineData.Type;

        // Parse date and time
        var date = DateOnly.ParseExact(lineData.Date, "yyyyMMdd");
        var time = TimeOnly.ParseExact(lineData.Time, "HHmmss");

        // Create value objects
        var amount = Money.FromCnabValue(lineData.ValueInCents);
        var cpf = CPF.CreateUnchecked(lineData.Cpf);
        var card = CardNumber.Create(lineData.CardNumber);

        // Create transaction entity
        return new FinancialTransaction
        {
            Type = transactionType,
            Date = date,
            Time = time,
            Amount = amount,
            Cpf = cpf,
            Card = card,
            Store = store,
            CreatedAt = DateTime.UtcNow
        };
    }
}