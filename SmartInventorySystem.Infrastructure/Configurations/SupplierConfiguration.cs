using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInventorySystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventorySystem.Infrastructure.Configurations
{

    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder.ToTable("Users");

            builder.Property(u => u.FirstName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(u => u.LastName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(u => u.Role)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Property(u => u.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(u => u.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // Indexes for better query performance
            builder.HasIndex(u => u.Email)
                .IsUnique();

            builder.HasIndex(u => u.Role);

            builder.HasIndex(u => u.IsActive);
        }
    }
}
