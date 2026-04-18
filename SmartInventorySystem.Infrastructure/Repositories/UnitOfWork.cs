using Microsoft.EntityFrameworkCore.Storage;
using SmartInventorySystem.Application.Interfaces;
using SmartInventorySystem.Domain.Entities;
using SmartInventorySystem.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Text;

using SmartInventorySystem.Infrastructure.Repositories;

namespace SmartInventorySystem.Infrastructure.Repositories
{

    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private IDbContextTransaction? _transaction;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        private IRepository<Product>? _products;
        private IRepository<Supplier>? _suppliers;
        private IRepository<Order>? _orders;
        private IRepository<OrderItem>? _orderItems;

        public IRepository<Product> Products => _products ??= new ProductRepository(_context);
        public IRepository<Supplier> Suppliers => _suppliers ??= new Repository<Supplier>(_context);
        public IRepository<Order> Orders => _orders ??= new Repository<Order>(_context);
        public IRepository<OrderItem> OrderItems => _orderItems ??= new Repository<OrderItem>(_context);

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}
