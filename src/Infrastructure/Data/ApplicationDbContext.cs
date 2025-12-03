using System.Reflection;
using DesafioDev.Application.Common.Interfaces;
using DesafioDev.Application.Common.Interfaces.Repositories;
using DesafioDev.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DesafioDev.Infrastructure.Data;

public class ApplicationDbContext : DbContext, IApplicationDbContext, IUnitOfWork
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Store> Stores => Set<Store>();
    public DbSet<FinancialTransaction> FinancialTransactions => Set<FinancialTransaction>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}