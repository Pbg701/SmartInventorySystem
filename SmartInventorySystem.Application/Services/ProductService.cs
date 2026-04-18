using AutoMapper;
using Microsoft.Extensions.Logging;
using SmartInventorySystem.Application.Common;
using SmartInventorySystem.Application.DTOs;
using SmartInventorySystem.Application.Interfaces;
using SmartInventorySystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventorySystem.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;
        private readonly ILogger<ProductService> _logger;
        private const string CachePrefix = "products";

        public ProductService(IUnitOfWork unitOfWork, IMapper mapper, ICacheService cacheService, ILogger<ProductService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<ApiResponse<PagedResult<ProductDto>>> GetAllProductsAsync(QueryParameters parameters)
        {
            try
            {
                var cacheKey = $"{CachePrefix}:page{parameters.PageNumber}:size{parameters.PageSize}:search{parameters.SearchTerm}:sort{parameters.SortBy}";

                var cachedResult = await _cacheService.GetAsync<PagedResult<ProductDto>>(cacheKey);
                if (cachedResult != null)
                {
                    return ApiResponse<PagedResult<ProductDto>>.SuccessResponse(cachedResult);
                }

                var products = await _unitOfWork.Products.GetPagedAsync(parameters);
                var productDtos = _mapper.Map<List<ProductDto>>(products.Items);

                var result = new PagedResult<ProductDto>
                {
                    Items = productDtos,
                    PageNumber = products.PageNumber,
                    PageSize = products.PageSize,
                    TotalCount = products.TotalCount
                };

                await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(10));

                return ApiResponse<PagedResult<ProductDto>>.SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all products");
                return ApiResponse<PagedResult<ProductDto>>.ErrorResponse("Failed to retrieve products");
            }
        }

        public async Task<ApiResponse<ProductDto>> GetProductByIdAsync(int id)
        {
            try
            {
                var cacheKey = $"{CachePrefix}:{id}";
                var cachedProduct = await _cacheService.GetAsync<ProductDto>(cacheKey);
                if (cachedProduct != null)
                {
                    return ApiResponse<ProductDto>.SuccessResponse(cachedProduct);
                }

                var product = await _unitOfWork.Products.GetByIdAsync(id);
                if (product == null)
                {
                    return ApiResponse<ProductDto>.ErrorResponse("Product not found");
                }

                var productDto = _mapper.Map<ProductDto>(product);
                await _cacheService.SetAsync(cacheKey, productDto, TimeSpan.FromMinutes(30));

                return ApiResponse<ProductDto>.SuccessResponse(productDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product by id: {Id}", id);
                return ApiResponse<ProductDto>.ErrorResponse("Failed to retrieve product");
            }
        }

        public async Task<ApiResponse<ProductDto>> CreateProductAsync(CreateProductDto createDto)
        {
            try
            {
                var existingProduct = await _unitOfWork.Products.ExistsAsync(p => p.Sku == createDto.Sku);
                if (existingProduct)
                {
                    return ApiResponse<ProductDto>.ErrorResponse("Product with this SKU already exists");
                }

                var product = _mapper.Map<Product>(createDto);
                var createdProduct = await _unitOfWork.Products.AddAsync(product);
                await _unitOfWork.SaveChangesAsync();

                await _cacheService.RemoveByPrefixAsync(CachePrefix);

                var productDto = _mapper.Map<ProductDto>(createdProduct);
                return ApiResponse<ProductDto>.SuccessResponse(productDto, "Product created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                return ApiResponse<ProductDto>.ErrorResponse("Failed to create product");
            }
        }

        public async Task<ApiResponse<ProductDto>> UpdateProductAsync(UpdateProductDto updateDto)
        {
            try
            {
                var product = await _unitOfWork.Products.GetByIdAsync(updateDto.Id);
                if (product == null)
                {
                    return ApiResponse<ProductDto>.ErrorResponse("Product not found");
                }

                _mapper.Map(updateDto, product);
                product.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.Products.UpdateAsync(product);
                await _unitOfWork.SaveChangesAsync();

                await _cacheService.RemoveByPrefixAsync(CachePrefix);
                await _cacheService.RemoveAsync($"{CachePrefix}:{product.Id}");

                var productDto = _mapper.Map<ProductDto>(product);
                return ApiResponse<ProductDto>.SuccessResponse(productDto, "Product updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product");
                return ApiResponse<ProductDto>.ErrorResponse("Failed to update product");
            }
        }

        public async Task<ApiResponse<bool>> DeleteProductAsync(int id)
        {
            try
            {
                var product = await _unitOfWork.Products.GetByIdAsync(id);
                if (product == null)
                {
                    return ApiResponse<bool>.ErrorResponse("Product not found");
                }

                await _unitOfWork.Products.DeleteAsync(product);
                await _unitOfWork.SaveChangesAsync();

                await _cacheService.RemoveByPrefixAsync(CachePrefix);
                await _cacheService.RemoveAsync($"{CachePrefix}:{id}");

                return ApiResponse<bool>.SuccessResponse(true, "Product deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product");
                return ApiResponse<bool>.ErrorResponse("Failed to delete product");
            }
        }

        //public async Task<ApiResponse<IEnumerable<ProductDto>>> GetLowStockProductsAsync()
        //{
        //    try
        //    {
        //        var products = await _unitOfWork.Products.FindAsync(p => p.IsLowStock);
        //        var productDtos = _mapper.Map<IEnumerable<ProductDto>>(products);
        //        return ApiResponse<IEnumerable<ProductDto>>.SuccessResponse(productDtos);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error getting low stock products");
        //        return ApiResponse<IEnumerable<ProductDto>>.ErrorResponse("Failed to retrieve low stock products");
        //    }
        //}
        public async Task<ApiResponse<IEnumerable<ProductDto>>> GetLowStockProductsAsync()
        {
            try
            {
                // FIXED: Use direct property comparison instead of computed property
                var products = await _unitOfWork.Products.FindAsync(p => p.StockQuantity <= p.MinimumStockThreshold);

                var productDtos = _mapper.Map<IEnumerable<ProductDto>>(products);

                // Ensure IsLowStock is set correctly in the DTO
                foreach (var dto in productDtos)
                {
                    dto.IsLowStock = dto.StockQuantity <= dto.MinimumStockThreshold;
                }

                return ApiResponse<IEnumerable<ProductDto>>.SuccessResponse(productDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting low stock products: {Message}", ex.Message);
                return ApiResponse<IEnumerable<ProductDto>>.ErrorResponse("Failed to retrieve low stock products");
            }
        }
    }
}
