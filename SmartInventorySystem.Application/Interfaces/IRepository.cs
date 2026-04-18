using SmartInventorySystem.Application.Common;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace SmartInventorySystem.Application.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task<PagedResult<T>> GetPagedAsync(QueryParameters parameters, Expression<Func<T, bool>>? filter = null);
        Task<T> AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
    }
}
