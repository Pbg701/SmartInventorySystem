using Microsoft.EntityFrameworkCore;
using SmartInventorySystem.Application.Common;
using SmartInventorySystem.Application.Interfaces;
using SmartInventorySystem.Domain.Entities;
using SmartInventorySystem.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace SmartInventorySystem.Infrastructure.Repositories
{

    public class Repository<T> : IRepository<T> where T : BaseEntity
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public async Task<PagedResult<T>> GetPagedAsync(QueryParameters parameters, Expression<Func<T, bool>>? filter = null)
        {
            var query = _dbSet.AsQueryable();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
            {
                // This would need to be overridden for specific entities with searchable properties
                // Base implementation doesn't apply search
            }

            var totalCount = await query.CountAsync();

            if (!string.IsNullOrWhiteSpace(parameters.SortBy))
            {
                query = parameters.SortDescending
                    ? query.OrderByDescending(e => EF.Property<object>(e, parameters.SortBy))
                    : query.OrderBy(e => EF.Property<object>(e, parameters.SortBy));
            }
            else
            {
                query = query.OrderByDescending(e => e.CreatedAt);
            }

            var items = await query
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync();

            return new PagedResult<T>
            {
                Items = items,
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize,
                TotalCount = totalCount
            };
        }

        public async Task<T> AddAsync(T entity)
        {
            entity.CreatedAt = DateTime.UtcNow;
            await _dbSet.AddAsync(entity);
            return entity;
        }

        public Task UpdateAsync(T entity)
        {
            entity.UpdatedAt = DateTime.UtcNow;
            _dbSet.Update(entity);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(T entity)
        {
            entity.IsDeleted = true;
            entity.DeletedAt = DateTime.UtcNow;
            _dbSet.Update(entity);
            return Task.CompletedTask;
        }

        public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }
    }
}
