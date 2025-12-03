using DesafioDev.Domain.Entities;
using DesafioDev.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DesafioDev.Infrastructure.Data.Configurations;

public class FinancialTransactionConfiguration : IEntityTypeConfiguration<FinancialTransaction>
{
    public void Configure(EntityTypeBuilder<FinancialTransaction> builder)
    {
        builder.ToTable("FinancialTransactions");

        builder.HasKey(t => t.Id);

        // Configure enum properties
        builder.Property(t => t.Type)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(t => t.Nature)
            .IsRequired()
            .HasConversion<int>();

        // Configure Date and Time properties
        builder.Property(t => t.Date)
            .IsRequired();

        builder.Property(t => t.Time)
            .IsRequired();

        // Configure Money value object - store as long (cents)
        builder.Property(t => t.Amount)
            .IsRequired()
            .HasConversion(
                v => v.Value,
                v => Money.FromCnabValue(v));

        // Configure CPF value object - store as string
        builder.Property(t => t.Cpf)
            .IsRequired()
            .HasConversion(
                v => v.Value,
                v => CPF.CreateUnchecked(v))
            .HasMaxLength(11)
            .IsFixedLength();

        // Configure CardNumber value object - store as string
        builder.Property(t => t.Card)
            .IsRequired()
            .HasConversion(
                v => v.Value,
                v => CardNumber.Create(v))
            .HasMaxLength(12)
            .IsFixedLength();

        // Configure Description
        builder.Property(t => t.Description)
            .IsRequired()
            .HasMaxLength(100);

        // Configure CreatedAt
        builder.Property(t => t.CreatedAt)
            .IsRequired();

        // Configure relationship with Store
        builder.HasOne(t => t.Store)
            .WithMany(s => s.Transactions)
            .HasForeignKey(t => t.StoreId)
            .OnDelete(DeleteBehavior.Cascade);

        // Add indexes for better query performance
        builder.HasIndex(t => t.Date);
        builder.HasIndex(t => t.StoreId);
        builder.HasIndex(t => t.Type);
        builder.HasIndex(t => new { t.StoreId, t.Date });
    }
}
