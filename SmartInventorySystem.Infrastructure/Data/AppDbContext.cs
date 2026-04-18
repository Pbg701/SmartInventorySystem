using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;
using SmartInventorySystem.Domain.Entities;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace SmartInventorySystem.Infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

            // Global soft-delete query filter
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                {
                    var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
                    var property = System.Linq.Expressions.Expression.Property(parameter, "IsDeleted");
                    var condition = System.Linq.Expressions.Expression.Equal(property, System.Linq.Expressions.Expression.Constant(false));
                    var lambda = System.Linq.Expressions.Expression.Lambda(condition, parameter);

                    entityType.SetQueryFilter(lambda);
                }
            }
        }
    }
}
