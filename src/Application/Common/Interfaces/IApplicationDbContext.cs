using DesafioDev.Domain.Entities;

namespace DesafioDev.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Store> Stores { get; }

    DbSet<FinancialTransaction> FinancialTransactions { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}