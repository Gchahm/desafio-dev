using DesafioDev.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DesafioDev.Infrastructure.Data.Configurations;

public class StoreConfiguration : IEntityTypeConfiguration<Store>
{
    public void Configure(EntityTypeBuilder<Store> builder)
    {
        builder.ToTable("Stores");

        builder.HasKey(s => s.Id);

        // Configure Name
        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(19); // From CNAB spec

        // Configure OwnerName
        builder.Property(s => s.OwnerName)
            .IsRequired()
            .HasMaxLength(14); // From CNAB spec

        // Configure relationship with FinancialTransactions
        builder.HasMany(s => s.Transactions)
            .WithOne(t => t.Store)
            .HasForeignKey(t => t.StoreId)
            .OnDelete(DeleteBehavior.Cascade);

        // Add unique index on store name
        builder.HasIndex(s => s.Name)
            .IsUnique();
    }
}
