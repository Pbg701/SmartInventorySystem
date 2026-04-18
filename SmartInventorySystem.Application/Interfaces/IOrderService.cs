using SmartInventorySystem.Application.Common;
using SmartInventorySystem.Application.DTOs;
using SmartInventorySystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventorySystem.Application.Interfaces
{

    public interface IOrderService
    {
        Task<ApiResponse<PagedResult<OrderDto>>> GetAllOrdersAsync(QueryParameters parameters);
        Task<ApiResponse<OrderDto>> GetOrderByIdAsync(int id);
        Task<ApiResponse<OrderDto>> CreateOrderAsync(CreateOrderDto createDto, int userId);
        Task<ApiResponse<OrderDto>> UpdateOrderStatusAsync(int id, OrderStatus status);
        Task<ApiResponse<bool>> CancelOrderAsync(int id);
    }
}
