using SmartInventorySystem.Application.Common;
using SmartInventorySystem.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventorySystem.Application.Interfaces
{
    public interface IProductService
    {
        Task<ApiResponse<PagedResult<ProductDto>>> GetAllProductsAsync(QueryParameters parameters);
        Task<ApiResponse<ProductDto>> GetProductByIdAsync(int id);
        Task<ApiResponse<ProductDto>> CreateProductAsync(CreateProductDto createDto);
        Task<ApiResponse<ProductDto>> UpdateProductAsync(UpdateProductDto updateDto);
        Task<ApiResponse<bool>> DeleteProductAsync(int id);
        Task<ApiResponse<IEnumerable<ProductDto>>> GetLowStockProductsAsync();
    }
}
