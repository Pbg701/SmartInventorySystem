using Microsoft.EntityFrameworkCore;
using SmartInventorySystem.Domain.Entities;
using SmartInventorySystem.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Text;
using SmartInventorySystem.Application.Common;

namespace SmartInventorySystem.Infrastructure.Repositories
{

    public class ProductRepository : Repository<Product>
    {
        public ProductRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<PagedResult<Product>> GetPagedAsync(QueryParameters parameters)
        {
            var query = _dbSet.Include(p => p.Supplier).AsQueryable();

            if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
            {
                query = query.Where(p =>
                    p.Name.Contains(parameters.SearchTerm) ||
                    p.Sku.Contains(parameters.SearchTerm) ||
                    p.Description.Contains(parameters.SearchTerm));
            }

            var totalCount = await query.CountAsync();

            if (!string.IsNullOrWhiteSpace(parameters.SortBy))
            {
                query = parameters.SortBy.ToLower() switch
                {
                    "name" => parameters.SortDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
                    "price" => parameters.SortDescending ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
                    "stockquantity" => parameters.SortDescending ? query.OrderByDescending(p => p.StockQuantity) : query.OrderBy(p => p.StockQuantity),
                    _ => parameters.SortDescending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt)
                };
            }
            else
            {
                query = query.OrderByDescending(p => p.CreatedAt);
            }

            var items = await query
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync();

            return new PagedResult<Product>
            {
                Items = items,
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize,
                TotalCount = totalCount
            };
        }
    }
}
