using DesafioDev.Domain.Entities;

namespace DesafioDev.Application.Common.Interfaces.Repositories;

public interface IFinancialTransactionRepository
{
    Task AddAsync(FinancialTransaction transaction, CancellationToken cancellationToken);
}
