using SmartInventorySystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventorySystem.Application.Interfaces
{

    public interface IUnitOfWork : IDisposable
    {
        IRepository<Product> Products { get; }
        IRepository<Supplier> Suppliers { get; }
        IRepository<Order> Orders { get; }
        IRepository<OrderItem> OrderItems { get; }

        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
