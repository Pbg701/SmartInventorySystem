using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInventorySystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventorySystem.Infrastructure.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("Products");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.Sku)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasIndex(p => p.Sku)
                .IsUnique();

            builder.Property(p => p.Description)
                .HasMaxLength(500);

            builder.Property(p => p.Price)
                .IsRequired()
                .HasPrecision(18, 2);

            builder.Property(p => p.StockQuantity)
                .IsRequired();

            builder.Property(p => p.MinimumStockThreshold)
                .IsRequired();

            builder.HasOne(p => p.Supplier)
                .WithMany(s => s.Products)
                .HasForeignKey(p => p.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
