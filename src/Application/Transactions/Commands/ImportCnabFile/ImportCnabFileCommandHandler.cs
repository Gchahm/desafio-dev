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
            var storesProcessed = new HashSet<string>();

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

                    storesProcessed.Add(storeName);

                    // Process each transaction for this store
                    foreach (var lineData in storeGroup)
                    {
                        try
                        {
                            var transaction = CreateTransaction(lineData, store);
                            await transactions.AddAsync(transaction, cancellationToken);
                            result = result with { SuccessfulImports = result.SuccessfulImports + 1 };
                        }
                        catch (Exception ex)
                        {
                            errors.Add($"Store '{storeName}': Failed to create transaction - {ex.Message}");
                            result = result with { FailedImports = result.FailedImports + 1 };
                        }
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"Store '{storeName}': {ex.Message}");
                    result = result with
                    {
                        FailedImports = result.FailedImports + storeGroup.Count()
                    };
                }
            }

            result = result with
            {
                StoresProcessed = storesProcessed.Count,
                Errors = errors
            };

            // Save all changes to database
            if (result.SuccessfulImports > 0)
            {
                await uow.SaveChangesAsync(cancellationToken);
            }

            return result;
        }
        catch (Exception ex)
        {
            errors.Add($"Critical error during import: {ex.Message}");
            return result with
            {
                Errors = errors,
                FailedImports = result.TotalLines - result.SuccessfulImports
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

        if (store == null)
        {
            // Create new store
            store = new Store
            {
                Name = storeName,
                OwnerName = storeOwner
            };

            await stores.AddAsync(store, cancellationToken);

            // Save to get the store ID immediately
            await uow.SaveChangesAsync(cancellationToken);
        }
        else if (store.OwnerName != storeOwner)
        {
            // Update owner name if changed
            store.OwnerName = storeOwner;
        }

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
            StoreId = store.Id,
            CreatedAt = DateTime.UtcNow
        };
    }
}