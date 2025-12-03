using DesafioDev.Application.Common.Interfaces.Repositories;
using DesafioDev.Domain.Entities;
using DesafioDev.Infrastructure.Data;

namespace DesafioDev.Infrastructure.Repositories;

public class FinancialTransactionRepository(ApplicationDbContext context) : IFinancialTransactionRepository
{
    public async Task AddAsync(FinancialTransaction transaction, CancellationToken cancellationToken)
    {
        await context.FinancialTransactions.AddAsync(transaction, cancellationToken);
    }
}
