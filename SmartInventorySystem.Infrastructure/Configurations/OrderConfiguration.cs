using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInventorySystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventorySystem.Infrastructure.Configurations
{

    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("Orders");

            builder.HasKey(o => o.Id);

            builder.Property(o => o.OrderNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasIndex(o => o.OrderNumber)
                .IsUnique();

            builder.Property(o => o.TotalAmount)
                .IsRequired()
                .HasPrecision(18, 2);

            builder.Property(o => o.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
