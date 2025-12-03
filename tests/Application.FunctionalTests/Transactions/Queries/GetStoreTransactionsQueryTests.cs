using DesafioDev.Application.Transactions.Queries.GetStoreTransactions;
using DesafioDev.Domain.Entities;
using DesafioDev.Domain.Enums;
using DesafioDev.Domain.ValueObjects;

namespace DesafioDev.Application.FunctionalTests.Transactions.Queries;

using static Testing;

public class GetStoreTransactionsQueryTests : BaseTestFixture
{
    [Test]
    public async Task ShouldReturnEmptyListWhenNoStoresExist()
    {
        // Arrange
        var query = new GetStoreTransactionsQuery();

        // Act
        var result = await SendAsync(query);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    [Test]
    public async Task ShouldReturnStoreWithNoTransactions()
    {
        // Arrange
        var store = CreateStore("BAR DO JOÃO", "JOÃO MACEDO");
        await AddAsync(store);

        var query = new GetStoreTransactionsQuery();

        // Act
        var result = await SendAsync(query);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);

        var storeDto = result.First();
        storeDto.Name.ShouldBe("BAR DO JOÃO");
        storeDto.OwnerName.ShouldBe("JOÃO MACEDO");
        storeDto.Transactions.ShouldBeEmpty();
        storeDto.TotalBalance.ShouldBe(0m);
    }

    [Test]
    public async Task ShouldReturnStoreWithSingleTransaction()
    {
        // Arrange
        var store = CreateStore("BAR DO JOÃO", "JOÃO MACEDO");
        await AddAsync(store);

        var transaction = CreateTransaction(
            store: store,
            type: TransactionType.Debit,
            amount: 15000, // R$ 150.00
            date: new DateOnly(2019, 3, 1),
            time: new TimeOnly(15, 30, 0));

        await AddAsync(transaction);

        var query = new GetStoreTransactionsQuery();

        // Act
        var result = await SendAsync(query);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);

        var storeDto = result.First();
        storeDto.Name.ShouldBe("BAR DO JOÃO");
        storeDto.Transactions.Count.ShouldBe(1);
        storeDto.TotalBalance.ShouldBe(150.00m);

        var transactionDto = storeDto.Transactions.First();
        transactionDto.Description.ShouldBe("Debit");
        transactionDto.Nature.ShouldBe("Income");
        transactionDto.SignedAmount.ShouldBe(150.00m); // Positive for income
    }

    [Test]
    public async Task ShouldReturnStoreWithMultipleTransactions()
    {
        // Arrange
        var store = CreateStore("BAR DO JOÃO", "JOÃO MACEDO");
        await AddAsync(store);

        var transaction1 = CreateTransaction(
            store: store,
            type: TransactionType.Debit,
            amount: 15000,
            date: new DateOnly(2019, 3, 1),
            time: new TimeOnly(15, 30, 0));

        var transaction2 = CreateTransaction(
            store: store,
            type: TransactionType.Financing,
            amount: 5000,
            date: new DateOnly(2019, 3, 1),
            time: new TimeOnly(16, 45, 0));

        var transaction3 = CreateTransaction(
            store: store,
            type: TransactionType.Credit,
            amount: 20000,
            date: new DateOnly(2019, 3, 2),
            time: new TimeOnly(10, 0, 0));

        await AddAsync(transaction1);
        await AddAsync(transaction2);
        await AddAsync(transaction3);

        var query = new GetStoreTransactionsQuery();

        // Act
        var result = await SendAsync(query);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);

        var storeDto = result.First();
        storeDto.Transactions.Count.ShouldBe(3);
        // Total: 150 - 50 + 200 = 300
        storeDto.TotalBalance.ShouldBe(300.00m);
    }

    [Test]
    public async Task ShouldReturnMultipleStoresWithTransactions()
    {
        // Arrange
        var store1 = CreateStore("BAR DO JOÃO", "JOÃO MACEDO");
        var store2 = CreateStore("LOJA DO Ó", "MARIA JOSEFINA");
        await AddAsync(store1);
        await AddAsync(store2);

        var transaction1 = CreateTransaction(
            store: store1,
            type: TransactionType.Debit,
            amount: 15000,
            date: new DateOnly(2019, 3, 1),
            time: new TimeOnly(15, 30, 0));

        var transaction2 = CreateTransaction(
            store: store2,
            type: TransactionType.Credit,
            amount: 25000,
            date: new DateOnly(2019, 3, 1),
            time: new TimeOnly(16, 0, 0));

        await AddAsync(transaction1);
        await AddAsync(transaction2);

        var query = new GetStoreTransactionsQuery();

        // Act
        var result = await SendAsync(query);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(2);

        var barDoJoao = result.First(s => s.Name == "BAR DO JOÃO");
        barDoJoao.Transactions.Count.ShouldBe(1);
        barDoJoao.TotalBalance.ShouldBe(150.00m);

        var lojaDoO = result.First(s => s.Name == "LOJA DO Ó");
        lojaDoO.Transactions.Count.ShouldBe(1);
        lojaDoO.TotalBalance.ShouldBe(250.00m);
    }

    [Test]
    public async Task ShouldOrderTransactionsByDateAndTimeDescending()
    {
        // Arrange
        var store = CreateStore("BAR DO JOÃO", "JOÃO MACEDO");
        await AddAsync(store);

        // Add transactions in non-chronological order
        var transaction1 = CreateTransaction(
            store: store,
            type: TransactionType.Debit,
            amount: 10000,
            date: new DateOnly(2019, 3, 1),
            time: new TimeOnly(10, 0, 0));

        var transaction2 = CreateTransaction(
            store: store,
            type: TransactionType.Credit,
            amount: 20000,
            date: new DateOnly(2019, 3, 3),
            time: new TimeOnly(14, 0, 0));

        var transaction3 = CreateTransaction(
            store: store,
            type: TransactionType.Sales,
            amount: 30000,
            date: new DateOnly(2019, 3, 2),
            time: new TimeOnly(12, 0, 0));

        var transaction4 = CreateTransaction(
            store: store,
            type: TransactionType.Boleto,
            amount: 5000,
            date: new DateOnly(2019, 3, 3),
            time: new TimeOnly(16, 0, 0)); // Same date as transaction2, but later time

        await AddAsync(transaction1);
        await AddAsync(transaction2);
        await AddAsync(transaction3);
        await AddAsync(transaction4);

        var query = new GetStoreTransactionsQuery();

        // Act
        var result = await SendAsync(query);

        // Assert
        var storeDto = result.First();
        storeDto.Transactions.Count.ShouldBe(4);

        // Should be ordered by date DESC, then time DESC
        // Expected order: transaction4 (3/3 16:00), transaction2 (3/3 14:00), transaction3 (3/2 12:00), transaction1 (3/1 10:00)
        storeDto.Transactions[0].Description.ShouldBe("Boleto"); // transaction4
        storeDto.Transactions[0].SignedAmount.ShouldBe(-50.00m);

        storeDto.Transactions[1].Description.ShouldBe("Credit"); // transaction2
        storeDto.Transactions[1].SignedAmount.ShouldBe(200.00m);

        storeDto.Transactions[2].Description.ShouldBe("Sales"); // transaction3
        storeDto.Transactions[2].SignedAmount.ShouldBe(300.00m);

        storeDto.Transactions[3].Description.ShouldBe("Debit"); // transaction1
        storeDto.Transactions[3].SignedAmount.ShouldBe(100.00m);
    }

    [Test]
    public async Task ShouldCalculateCorrectBalanceWithIncomeAndExpenses()
    {
        // Arrange
        var store = CreateStore("BAR DO JOÃO", "JOÃO MACEDO");
        await AddAsync(store);

        // Income transactions
        await AddAsync(CreateTransaction(store, TransactionType.Debit, 15000));
        await AddAsync(CreateTransaction(store, TransactionType.Credit, 20000));
        await AddAsync(CreateTransaction(store, TransactionType.Sales, 10000));

        // Expense transactions
        await AddAsync(CreateTransaction(store, TransactionType.Financing, 5000));
        await AddAsync(CreateTransaction(store, TransactionType.Boleto, 3000));
        await AddAsync(CreateTransaction(store, TransactionType.Rent, 12000));

        var query = new GetStoreTransactionsQuery();

        // Act
        var result = await SendAsync(query);

        // Assert
        var storeDto = result.First();
        storeDto.Transactions.Count.ShouldBe(6);
        // Total: (150 + 200 + 100) - (50 + 30 + 120) = 450 - 200 = 250
        storeDto.TotalBalance.ShouldBe(250.00m);
    }

    [Test]
    public async Task ShouldReturnNegativeBalanceWhenExpensesExceedIncome()
    {
        // Arrange
        var store = CreateStore("BAR DO JOÃO", "JOÃO MACEDO");
        await AddAsync(store);

        // Income: 100
        await AddAsync(CreateTransaction(store, TransactionType.Debit, 10000));

        // Expenses: 300
        await AddAsync(CreateTransaction(store, TransactionType.Financing, 15000));
        await AddAsync(CreateTransaction(store, TransactionType.Rent, 15000));

        var query = new GetStoreTransactionsQuery();

        // Act
        var result = await SendAsync(query);

        // Assert
        var storeDto = result.First();
        // Total: 100 - 300 = -200
        storeDto.TotalBalance.ShouldBe(-200.00m);
    }

    [Test]
    public async Task ShouldMapAllTransactionFieldsCorrectly()
    {
        // Arrange
        var store = CreateStore("BAR DO JOÃO", "JOÃO MACEDO");
        await AddAsync(store);

        var expectedDate = new DateOnly(2019, 3, 1);
        var expectedTime = new TimeOnly(15, 34, 53);
        var cpf = CPF.Create("09620676017");
        var card = CardNumber.Create("4753****3153");

        var transaction = new FinancialTransaction
        {
            StoreId = store.Id,
            Type = TransactionType.Financing,
            Amount = Money.FromCnabValue(14200),
            Date = expectedDate,
            Time = expectedTime,
            Cpf = cpf,
            Card = card
        };

        await AddAsync(transaction);

        var query = new GetStoreTransactionsQuery();

        // Act
        var result = await SendAsync(query);

        // Assert
        var storeDto = result.First();
        var transactionDto = storeDto.Transactions.First();

        transactionDto.Description.ShouldBe("Financing");
        transactionDto.Nature.ShouldBe("Expense");
        transactionDto.Date.ShouldBe(new DateTime(2019, 3, 1, 15, 34, 53));
        transactionDto.SignedAmount.ShouldBe(-142.00m); // Negative for expense
        transactionDto.Cpf.ShouldBe("096.206.760-17"); // Formatted CPF
        transactionDto.CardNumber.ShouldBe("4753-****-3153"); // Masked card
    }

    [Test]
    public async Task ShouldMapStoreFieldsCorrectly()
    {
        // Arrange
        var store = CreateStore("BAR DO JOÃO", "JOÃO MACEDO");
        await AddAsync(store);

        var query = new GetStoreTransactionsQuery();

        // Act
        var result = await SendAsync(query);

        // Assert
        result.Count.ShouldBe(1);
        var storeDto = result.First();

        storeDto.Id.ShouldBeGreaterThan(0);
        storeDto.Name.ShouldBe("BAR DO JOÃO");
        storeDto.OwnerName.ShouldBe("JOÃO MACEDO");
        storeDto.Transactions.ShouldNotBeNull();
        storeDto.TotalBalance.ShouldBe(0m);
    }

    [Test]
    public async Task ShouldIncludeAllTransactionTypesWithCorrectDescriptions()
    {
        // Arrange
        var store = CreateStore("TEST STORE", "TEST OWNER");
        await AddAsync(store);

        // Add one transaction of each type
        await AddAsync(CreateTransaction(store, TransactionType.Debit, 1000));
        await AddAsync(CreateTransaction(store, TransactionType.Boleto, 1000));
        await AddAsync(CreateTransaction(store, TransactionType.Financing, 1000));
        await AddAsync(CreateTransaction(store, TransactionType.Credit, 1000));
        await AddAsync(CreateTransaction(store, TransactionType.LoanReceipt, 1000));
        await AddAsync(CreateTransaction(store, TransactionType.Sales, 1000));
        await AddAsync(CreateTransaction(store, TransactionType.TedReceipt, 1000));
        await AddAsync(CreateTransaction(store, TransactionType.DocReceipt, 1000));
        await AddAsync(CreateTransaction(store, TransactionType.Rent, 1000));

        var query = new GetStoreTransactionsQuery();

        // Act
        var result = await SendAsync(query);

        // Assert
        var storeDto = result.First();
        storeDto.Transactions.Count.ShouldBe(9);

        var descriptions = storeDto.Transactions.Select(t => t.Description).ToList();
        descriptions.ShouldContain("Debit");
        descriptions.ShouldContain("Boleto");
        descriptions.ShouldContain("Financing");
        descriptions.ShouldContain("Credit");
        descriptions.ShouldContain("Loan Receipt");
        descriptions.ShouldContain("Sales");
        descriptions.ShouldContain("TED Receipt");
        descriptions.ShouldContain("DOC Receipt");
        descriptions.ShouldContain("Rent");

        // Total balance: (10 + 10 + 10 + 10 + 10 + 10) - (10 + 10 + 10) = 60 - 30 = 30
        storeDto.TotalBalance.ShouldBe(30.00m);
    }

    #region Helper Methods

    private static Store CreateStore(string name, string ownerName)
    {
        return new Store
        {
            Name = name,
            OwnerName = ownerName
        };
    }

    private static FinancialTransaction CreateTransaction(
        Store store,
        TransactionType type,
        long amount,
        DateOnly? date = null,
        TimeOnly? time = null)
    {
        return new FinancialTransaction
        {
            StoreId = store.Id,
            Type = type,
            Amount = Money.FromCnabValue(amount),
            Date = date ?? new DateOnly(2019, 3, 1),
            Time = time ?? new TimeOnly(12, 0, 0),
            Cpf = CPF.Create("09620676017"),
            Card = CardNumber.Create("4753****3153")
        };
    }

    #endregion
}
