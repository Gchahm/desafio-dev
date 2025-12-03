using System.Text;
using DesafioDev.Application.Transactions.Commands.ImportCnabFile;
using DesafioDev.Domain.Entities;
using DesafioDev.Domain.Enums;

namespace DesafioDev.Application.FunctionalTests.Transactions.Commands;

using static Testing;

public class ImportCnabFileTests : BaseTestFixture
{
    [Test]
    public async Task ShouldImportValidCnabFile()
    {
        // Arrange
        var cnabContent = CreateValidCnabContent();
        var stream = CreateStreamFromString(cnabContent);

        var command = new ImportCnabFileCommand
        {
            FileStream = stream,
            FileName = "test.txt"
        };

        // Act
        var result = await SendAsync(command);

        // Assert
        result.ShouldNotBeNull();
        result.TotalLines.ShouldBe(3);
        result.ValidLines.ShouldBe(3);
        result.InvalidLines.ShouldBe(0);
        result.IsSuccess.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();

        // Verify stores were created
        var storeCount = await CountAsync<Store>();
        storeCount.ShouldBe(2);

        // Verify transactions were created
        var transactionCount = await CountAsync<FinancialTransaction>();
        transactionCount.ShouldBe(3);
    }

    [Test]
    public async Task ShouldCreateStoreWithCorrectData()
    {
        // Arrange
        var cnabContent = CreateValidCnabContent();
        var stream = CreateStreamFromString(cnabContent);

        var command = new ImportCnabFileCommand
        {
            FileStream = stream,
            FileName = "test.txt"
        };

        // Act
        await SendAsync(command);

        // Assert
        var stores = await GetAllAsync<Store>();
        stores.ShouldNotBeNull();
        stores.Count.ShouldBe(2);

        var barDoJoao = stores.FirstOrDefault(s => s.Name == "BAR DO JOÃO");
        barDoJoao.ShouldNotBeNull();
        barDoJoao!.OwnerName.ShouldBe("JOÃO MACEDO");

        var lojaDoO = stores.FirstOrDefault(s => s.Name == "LOJA DO Ó - MATRIZ");
        lojaDoO.ShouldNotBeNull();
        lojaDoO!.OwnerName.ShouldBe("MARIA JOSEFINA");
    }

    [Test]
    public async Task ShouldCreateTransactionsWithCorrectData()
    {
        // Arrange
        var cnabContent = CreateValidCnabContent();
        var stream = CreateStreamFromString(cnabContent);

        var command = new ImportCnabFileCommand
        {
            FileStream = stream,
            FileName = "test.txt"
        };

        // Act
        await SendAsync(command);

        // Assert
        var allTransactions = await GetAllAsync<FinancialTransaction>();
        allTransactions.ShouldNotBeNull();
        allTransactions.Count.ShouldBe(3);

        // Check transaction with Type 3 (Financing - Expense)
        var transaction1 = allTransactions.First(t => t.Type == TransactionType.Financing);
        transaction1.Nature.ShouldBe(TransactionNature.Expense);
        transaction1.Amount.ToDecimal().ShouldBe(142.00m);
        transaction1.Date.ShouldBe(new DateOnly(2019, 3, 1));
        transaction1.Time.ShouldBe(new TimeOnly(15, 34, 53));
        transaction1.Cpf.Value.ShouldBe("09620676017");
        transaction1.Card.Value.ShouldBe("4753****3153");
        transaction1.Description.ShouldBe("Financing");

        // Check transaction with Type 5 (Loan Receipt - Income)
        var transaction2 = allTransactions.First(t => t.Type == TransactionType.LoanReceipt);
        transaction2.Nature.ShouldBe(TransactionNature.Income);
        transaction2.Amount.ToDecimal().ShouldBe(132.00m);

        // Check transaction with Type 1 (Debit - Income)
        var transaction3 = allTransactions.First(t => t.Type == TransactionType.Debit);
        transaction3.Nature.ShouldBe(TransactionNature.Income);
        transaction3.Amount.ToDecimal().ShouldBe(152.00m);
    }

    [Test]
    public async Task ShouldHandleDuplicateStoreImport()
    {
        // Arrange - First import
        var cnabContent1 = CreateSingleTransactionCnab("BAR DO JOÃO", "JOÃO MACEDO");
        var stream1 = CreateStreamFromString(cnabContent1);
        await SendAsync(new ImportCnabFileCommand { FileStream = stream1, FileName = "test1.txt" });

        // Act - Second import with same store
        var cnabContent2 = CreateSingleTransactionCnab("BAR DO JOÃO", "JOÃO MACEDO");
        var stream2 = CreateStreamFromString(cnabContent2);
        var result = await SendAsync(new ImportCnabFileCommand { FileStream = stream2, FileName = "test2.txt" });

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.ValidLines.ShouldBe(1);

        // Should still have only 1 store
        var storeCount = await CountAsync<Store>();
        storeCount.ShouldBe(1);

        // But should have 2 transactions
        var transactionCount = await CountAsync<FinancialTransaction>();
        transactionCount.ShouldBe(2);
    }

    [Test]
    public async Task ShouldHandleEmptyFile()
    {
        // Arrange
        var stream = CreateStreamFromString(string.Empty);
        var command = new ImportCnabFileCommand
        {
            FileStream = stream,
            FileName = "empty.txt"
        };

        // Act
        var result = await SendAsync(command);

        // Assert
        result.TotalLines.ShouldBe(0);
        result.ValidLines.ShouldBe(0);
        result.InvalidLines.ShouldBe(0);
        result.Errors.Count.ShouldBe(1);
        result.Errors[0].ShouldContain("empty", Case.Insensitive);

        // No transactions should be created
        var transactionCount = await CountAsync<FinancialTransaction>();
        transactionCount.ShouldBe(0);
    }

    [Test]
    public async Task ShouldHandleFileWithEmptyLines()
    {
        // Arrange
        var cnabContent = CreateValidCnabContent() + "\n\n\n";
        var stream = CreateStreamFromString(cnabContent);
        var command = new ImportCnabFileCommand
        {
            FileStream = stream,
            FileName = "test.txt"
        };

        // Act
        var result = await SendAsync(command);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.ValidLines.ShouldBe(3); // Empty lines should be skipped
        result.TotalLines.ShouldBe(3);
    }

    [Test]
    public async Task ShouldHandlePartiallyValidFile()
    {
        // Arrange - File with 2 valid lines and 1 invalid line
        var validLine1 = CreateCnabLine(type: 1, storeName: "BAR DO JOÃO", storeOwner: "JOÃO MACEDO");
        var invalidLine = "INVALID_LINE_TOO_SHORT";
        var validLine2 = CreateCnabLine(type: 2, storeName: "LOJA DO Ó", storeOwner: "MARIA");

        var cnabContent = $"{validLine1}\n{invalidLine}\n{validLine2}";
        var stream = CreateStreamFromString(cnabContent);
        var command = new ImportCnabFileCommand
        {
            FileStream = stream,
            FileName = "partial.txt"
        };

        // Act
        var result = await SendAsync(command);

        // Assert - Parser will fail when encountering invalid line
        result.TotalLines.ShouldBe(0);
        result.ValidLines.ShouldBe(0);
        result.Errors.ShouldNotBeEmpty();
        result.Errors.Any(e => e.Contains("line 2", StringComparison.OrdinalIgnoreCase)).ShouldBeTrue();
    }

    [Test]
    public async Task ShouldCalculateCorrectTransactionNature()
    {
        // Arrange - Test all transaction types
        var lines = new[]
        {
            CreateCnabLine(type: 1, storeName: "STORE1", storeOwner: "OWNER1"), // Debit - Income
            CreateCnabLine(type: 2, storeName: "STORE2", storeOwner: "OWNER2"), // Boleto - Expense
            CreateCnabLine(type: 3, storeName: "STORE3", storeOwner: "OWNER3"), // Financing - Expense
            CreateCnabLine(type: 4, storeName: "STORE4", storeOwner: "OWNER4"), // Credit - Income
            CreateCnabLine(type: 5, storeName: "STORE5", storeOwner: "OWNER5"), // Loan Receipt - Income
            CreateCnabLine(type: 6, storeName: "STORE6", storeOwner: "OWNER6"), // Sales - Income
            CreateCnabLine(type: 7, storeName: "STORE7", storeOwner: "OWNER7"), // TED Receipt - Income
            CreateCnabLine(type: 8, storeName: "STORE8", storeOwner: "OWNER8"), // DOC Receipt - Income
            CreateCnabLine(type: 9, storeName: "STORE9", storeOwner: "OWNER9")  // Rent - Expense
        };

        var cnabContent = string.Join("\n", lines);
        var stream = CreateStreamFromString(cnabContent);
        var command = new ImportCnabFileCommand { FileStream = stream, FileName = "test.txt" };

        // Act
        await SendAsync(command);

        // Assert
        var transactions = await GetAllAsync<FinancialTransaction>();
        transactions.Count.ShouldBe(9);

        // Verify natures
        transactions.Single(t => t.Type == TransactionType.Debit).Nature.ShouldBe(TransactionNature.Income);
        transactions.Single(t => t.Type == TransactionType.Boleto).Nature.ShouldBe(TransactionNature.Expense);
        transactions.Single(t => t.Type == TransactionType.Financing).Nature.ShouldBe(TransactionNature.Expense);
        transactions.Single(t => t.Type == TransactionType.Credit).Nature.ShouldBe(TransactionNature.Income);
        transactions.Single(t => t.Type == TransactionType.LoanReceipt).Nature.ShouldBe(TransactionNature.Income);
        transactions.Single(t => t.Type == TransactionType.Sales).Nature.ShouldBe(TransactionNature.Income);
        transactions.Single(t => t.Type == TransactionType.TedReceipt).Nature.ShouldBe(TransactionNature.Income);
        transactions.Single(t => t.Type == TransactionType.DocReceipt).Nature.ShouldBe(TransactionNature.Income);
        transactions.Single(t => t.Type == TransactionType.Rent).Nature.ShouldBe(TransactionNature.Expense);
    }

    [Test]
    public async Task ShouldStoreAmountInCents()
    {
        // Arrange
        var line = CreateCnabLine(type: 1, value: 123456, storeName: "STORE", storeOwner: "OWNER");
        var stream = CreateStreamFromString(line);
        var command = new ImportCnabFileCommand { FileStream = stream, FileName = "test.txt" };

        // Act
        await SendAsync(command);

        // Assert
        var transaction = (await GetAllAsync<FinancialTransaction>()).First();
        transaction.Amount.Value.ShouldBe(123456L); // Internal storage is in cents
        transaction.Amount.ToDecimal().ShouldBe(1234.56m); // Decimal representation
    }

    [Test]
    public async Task ShouldLinkTransactionsToCorrectStore()
    {
        // Arrange
        var cnabContent = CreateValidCnabContent();
        var stream = CreateStreamFromString(cnabContent);
        var command = new ImportCnabFileCommand { FileStream = stream, FileName = "test.txt" };

        // Act
        await SendAsync(command);

        // Assert
        var barDoJoao = (await GetAllAsync<Store>()).First(s => s.Name == "BAR DO JOÃO");
        var barTransactions = (await GetAllAsync<FinancialTransaction>())
            .Where(t => t.StoreId == barDoJoao.Id)
            .ToList();

        barTransactions.Count.ShouldBe(2);
        barTransactions.All(t => t.StoreId == barDoJoao.Id).ShouldBeTrue();
    }

    [Test]
    public async Task ShouldNotSaveAnyTransactionsWhenFileHasInvalidTransactionsAndIgnoreErrorsIsFalse()
    {
        // Arrange - File with 2 valid transactions and 1 invalid transaction (invalid type)
        var validLine1 = CreateCnabLine(type: 1, storeName: "STORE1", storeOwner: "OWNER1");
        var invalidLine = CreateCnabLine(type: 0, storeName: "STORE2", storeOwner: "OWNER2"); // Invalid transaction type (0 is not defined)
        var validLine2 = CreateCnabLine(type: 3, storeName: "STORE3", storeOwner: "OWNER3");

        var cnabContent = $"{validLine1}\n{invalidLine}\n{validLine2}";
        var stream = CreateStreamFromString(cnabContent);
        var command = new ImportCnabFileCommand
        {
            FileStream = stream,
            FileName = "mixed.txt",
            IgnoreErrors = false // Explicit false
        };

        // Act
        var result = await SendAsync(command);

        // Assert
        result.TotalLines.ShouldBe(3);
        result.ValidLines.ShouldBe(2);
        result.InvalidLines.ShouldBe(1);
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();
        result.Errors.Any(e => e.Contains("Invalid transaction type", StringComparison.OrdinalIgnoreCase)).ShouldBeTrue();

        // CRITICAL: No stores or transactions should be saved to database
        var storeCount = await CountAsync<Store>();
        storeCount.ShouldBe(0);

        var transactionCount = await CountAsync<FinancialTransaction>();
        transactionCount.ShouldBe(0);
    }

    [Test]
    public async Task ShouldSaveValidTransactionsWhenFileHasInvalidTransactionsAndIgnoreErrorsIsTrue()
    {
        // Arrange - File with 2 valid transactions and 1 invalid transaction (invalid type)
        var validLine1 = CreateCnabLine(type: 1, storeName: "STORE1", storeOwner: "OWNER1");
        var invalidLine = CreateCnabLine(type: 0, storeName: "STORE2", storeOwner: "OWNER2"); // Invalid transaction type (0 is not defined)
        var validLine2 = CreateCnabLine(type: 3, storeName: "STORE3", storeOwner: "OWNER3");

        var cnabContent = $"{validLine1}\n{invalidLine}\n{validLine2}";
        var stream = CreateStreamFromString(cnabContent);
        var command = new ImportCnabFileCommand
        {
            FileStream = stream,
            FileName = "mixed.txt",
            IgnoreErrors = true // Ignore errors and save valid transactions
        };

        // Act
        var result = await SendAsync(command);

        // Assert
        result.TotalLines.ShouldBe(3);
        result.ValidLines.ShouldBe(2);
        result.InvalidLines.ShouldBe(1);
        result.IsSuccess.ShouldBeTrue();
        result.Errors.ShouldNotBeEmpty();
        result.Errors.Any(e => e.Contains("Invalid transaction type", StringComparison.OrdinalIgnoreCase)).ShouldBeTrue();

        // CRITICAL: Valid stores and transactions SHOULD be saved to database
        // Note: STORE2 is also created (even though its transaction failed) because
        // store creation happens before transaction processing
        var storeCount = await CountAsync<Store>();
        storeCount.ShouldBe(3); // STORE1, STORE2, and STORE3

        var transactionCount = await CountAsync<FinancialTransaction>();
        transactionCount.ShouldBe(2); // Only the 2 valid transactions (STORE2's transaction failed)

        // Verify the valid transactions were saved correctly
        var transactions = await GetAllAsync<FinancialTransaction>();
        transactions.Any(t => t.Type == TransactionType.Debit).ShouldBeTrue(); // type 1
        transactions.Any(t => t.Type == TransactionType.Financing).ShouldBeTrue(); // type 3

        // Verify STORE2 exists but has no transactions
        var store2 = (await GetAllAsync<Store>()).FirstOrDefault(s => s.Name == "STORE2");
        store2.ShouldNotBeNull();
        transactions.Any(t => t.StoreId == store2!.Id).ShouldBeFalse(); // No transactions for STORE2
    }

    [Test]
    public async Task ShouldNotSaveAnythingWhenAllTransactionsFailAndIgnoreErrorsIsTrue()
    {
        // Arrange - File with only invalid transactions
        var invalidLine1 = CreateCnabLine(type: 0, storeName: "STORE1", storeOwner: "OWNER1");
        var invalidLine2 = CreateCnabLine(type: 0, storeName: "STORE2", storeOwner: "OWNER2");

        var cnabContent = $"{invalidLine1}\n{invalidLine2}";
        var stream = CreateStreamFromString(cnabContent);
        var command = new ImportCnabFileCommand
        {
            FileStream = stream,
            FileName = "allinvalid.txt",
            IgnoreErrors = true
        };

        // Act
        var result = await SendAsync(command);

        // Assert
        result.TotalLines.ShouldBe(2);
        result.ValidLines.ShouldBe(0);
        result.InvalidLines.ShouldBe(2);
        result.IsSuccess.ShouldBeFalse();

        // Nothing should be saved because there are no successful imports
        var storeCount = await CountAsync<Store>();
        storeCount.ShouldBe(0);

        var transactionCount = await CountAsync<FinancialTransaction>();
        transactionCount.ShouldBe(0);
    }

    [Test]
    public async Task ShouldNotSaveWhenOnlyStoreCreationFailsAndIgnoreErrorsIsFalse()
    {
        // Arrange - Create a file where one store group will fail completely
        var validLine1 = CreateCnabLine(type: 1, storeName: "STORE1", storeOwner: "OWNER1");
        var validLine2 = CreateCnabLine(type: 1, storeName: "STORE1", storeOwner: "OWNER1");
        var invalidLine = CreateCnabLine(type: 0, storeName: "STORE2", storeOwner: "OWNER2"); // This will fail

        var cnabContent = $"{validLine1}\n{validLine2}\n{invalidLine}";
        var stream = CreateStreamFromString(cnabContent);
        var command = new ImportCnabFileCommand
        {
            FileStream = stream,
            FileName = "mixed.txt",
            IgnoreErrors = false
        };

        // Act
        var result = await SendAsync(command);

        // Assert
        result.ValidLines.ShouldBe(2);
        result.InvalidLines.ShouldBe(1);

        // No data should be saved because IgnoreErrors is false and there are failures
        var storeCount = await CountAsync<Store>();
        storeCount.ShouldBe(0);

        var transactionCount = await CountAsync<FinancialTransaction>();
        transactionCount.ShouldBe(0);
    }

    #region Helper Methods

    /// <summary>
    /// Creates valid CNAB content with 3 transactions (2 for BAR DO JOÃO, 1 for LOJA DO Ó)
    /// </summary>
    private static string CreateValidCnabContent()
    {
        var line1 = CreateCnabLine(
            type: 3,
            date: "20190301",
            value: 14200,
            cpf: "09620676017",
            card: "4753****3153",
            time: "153453",
            storeOwner: "JOÃO MACEDO",
            storeName: "BAR DO JOÃO");

        var line2 = CreateCnabLine(
            type: 5,
            date: "20190301",
            value: 13200,
            cpf: "55641815063",
            card: "3123****7687",
            time: "145607",
            storeOwner: "MARIA JOSEFINA",
            storeName: "LOJA DO Ó - MATRIZ");

        var line3 = CreateCnabLine(
            type: 1,
            date: "20190301",
            value: 15200,
            cpf: "09620676017",
            card: "1234****7890",
            time: "233000",
            storeOwner: "JOÃO MACEDO",
            storeName: "BAR DO JOÃO");

        return $"{line1}\n{line2}\n{line3}";
    }

    private static string CreateSingleTransactionCnab(string storeName, string storeOwner)
    {
        return CreateCnabLine(type: 1, storeName: storeName, storeOwner: storeOwner);
    }

    /// <summary>
    /// Creates a valid CNAB line with exactly 81 characters
    /// </summary>
    private static string CreateCnabLine(
        int type = 1,
        string date = "20190301",
        long value = 142000,
        string cpf = "09620676017",
        string card = "4753****3153",
        string time = "153453",
        string storeOwner = "JOÃO MACEDO",
        string storeName = "BAR DO JOÃO")
    {
        var typeStr = type.ToString().PadLeft(1, '0');
        var dateStr = date.PadRight(8);
        var valueStr = value.ToString().PadLeft(10, '0');
        var cpfStr = cpf.PadRight(11);
        var cardStr = card.PadRight(12);
        var timeStr = time.PadRight(6);
        var ownerStr = storeOwner.PadRight(14);
        var nameStr = storeName.PadRight(19);

        var line = typeStr + dateStr + valueStr + cpfStr + cardStr + timeStr + ownerStr + nameStr;

        if (line.Length != 81)
            throw new InvalidOperationException($"Generated line is {line.Length} characters, expected 81");

        return line;
    }

    private static Stream CreateStreamFromString(string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        return new MemoryStream(bytes);
    }

    #endregion
}
