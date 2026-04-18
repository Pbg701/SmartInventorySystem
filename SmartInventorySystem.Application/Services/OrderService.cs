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

    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<OrderService> _logger;

        public OrderService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<OrderService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ApiResponse<PagedResult<OrderDto>>> GetAllOrdersAsync(QueryParameters parameters)
        {
            try
            {
                var orders = await _unitOfWork.Orders.GetPagedAsync(parameters);
                var orderDtos = _mapper.Map<List<OrderDto>>(orders.Items);

                var result = new PagedResult<OrderDto>
                {
                    Items = orderDtos,
                    PageNumber = orders.PageNumber,
                    PageSize = orders.PageSize,
                    TotalCount = orders.TotalCount
                };

                return ApiResponse<PagedResult<OrderDto>>.SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all orders");
                return ApiResponse<PagedResult<OrderDto>>.ErrorResponse("Failed to retrieve orders");
            }
        }

        public async Task<ApiResponse<OrderDto>> GetOrderByIdAsync(int id)
        {
            try
            {
                var order = await _unitOfWork.Orders.GetByIdAsync(id);
                if (order == null)
                {
                    return ApiResponse<OrderDto>.ErrorResponse("Order not found");
                }

                var orderDto = _mapper.Map<OrderDto>(order);
                return ApiResponse<OrderDto>.SuccessResponse(orderDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order by id: {Id}", id);
                return ApiResponse<OrderDto>.ErrorResponse("Failed to retrieve order");
            }
        }
        public async Task<ApiResponse<OrderDto>> CreateOrderAsync(CreateOrderDto createDto, int userId)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var order = new Order
                {
                    OrderNumber = GenerateOrderNumber(),
                    OrderDate = DateTime.UtcNow,
                    UserId = userId,
                    Status = OrderStatus.Pending,
                    TotalAmount = 0
                };

                var createdOrder = await _unitOfWork.Orders.AddAsync(order);
                await _unitOfWork.SaveChangesAsync();

                decimal totalAmount = 0;
                var orderItems = new List<OrderItem>();

                foreach (var item in createDto.Items)
                {
                    var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
                    if (product == null)
                    {
                        return ApiResponse<OrderDto>.ErrorResponse($"Product with id {item.ProductId} not found");
                    }

                    if (product.StockQuantity < item.Quantity)
                    {
                        return ApiResponse<OrderDto>.ErrorResponse($"Insufficient stock for product {product.Name}");
                    }

                    var orderItem = new OrderItem
                    {
                        OrderId = createdOrder.Id,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = product.Price
                    };

                    orderItems.Add(orderItem);
                    totalAmount += orderItem.Subtotal;

                    product.StockQuantity -= item.Quantity;
                    await _unitOfWork.Products.UpdateAsync(product);
                }

                foreach (var orderItem in orderItems)
                {
                    await _unitOfWork.OrderItems.AddAsync(orderItem);
                }

                createdOrder.TotalAmount = totalAmount;
                await _unitOfWork.Orders.UpdateAsync(createdOrder);
                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitTransactionAsync();

                // ========== CORRECTED FIX ==========
                // Instead of trying to attach items to existing object,
                // fetch the complete order with all its related data

                var completeOrder = await _unitOfWork.Orders.GetByIdAsync(createdOrder.Id);
                if (completeOrder != null)
                {
                    // Load the order items
                    var items = await _unitOfWork.OrderItems.FindAsync(oi => oi.OrderId == completeOrder.Id);
                    completeOrder.OrderItems = items.ToList();

                    // Load product details for each item (for product name)
                    foreach (var item in completeOrder.OrderItems)
                    {
                        item.Product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
                    }
                }

                var orderDto = _mapper.Map<OrderDto>(completeOrder ?? createdOrder);
                // ========== END FIX ==========

                return ApiResponse<OrderDto>.SuccessResponse(orderDto, "Order created successfully");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error creating order");
                return ApiResponse<OrderDto>.ErrorResponse("Failed to create order");
            }
        }
        //public async Task<ApiResponse<OrderDto>> CreateOrderAsync(CreateOrderDto createDto, int userId)
        //{
        //    await _unitOfWork.BeginTransactionAsync();

        //    try
        //    {
        //        var order = new Order
        //        {
        //            OrderNumber = GenerateOrderNumber(),
        //            OrderDate = DateTime.UtcNow,
        //            UserId = userId,
        //            Status = OrderStatus.Pending,
        //            TotalAmount = 0
        //        };

        //        var createdOrder = await _unitOfWork.Orders.AddAsync(order);
        //        await _unitOfWork.SaveChangesAsync();

        //        decimal totalAmount = 0;
        //        var orderItems = new List<OrderItem>();

        //        foreach (var item in createDto.Items)
        //        {
        //            var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
        //            if (product == null)
        //            {
        //                return ApiResponse<OrderDto>.ErrorResponse($"Product with id {item.ProductId} not found");
        //            }

        //            if (product.StockQuantity < item.Quantity)
        //            {
        //                return ApiResponse<OrderDto>.ErrorResponse($"Insufficient stock for product {product.Name}");
        //            }

        //            var orderItem = new OrderItem
        //            {
        //                OrderId = createdOrder.Id,
        //                ProductId = item.ProductId,
        //                Quantity = item.Quantity,
        //                UnitPrice = product.Price
        //            };

        //            orderItems.Add(orderItem);
        //            totalAmount += orderItem.Subtotal;

        //            product.StockQuantity -= item.Quantity;
        //            await _unitOfWork.Products.UpdateAsync(product);
        //        }

        //        foreach (var orderItem in orderItems)
        //        {
        //            await _unitOfWork.OrderItems.AddAsync(orderItem);
        //        }

        //        createdOrder.TotalAmount = totalAmount;
        //        await _unitOfWork.Orders.UpdateAsync(createdOrder);
        //        await _unitOfWork.SaveChangesAsync();

        //        await _unitOfWork.CommitTransactionAsync();

        //        var orderDto = _mapper.Map<OrderDto>(createdOrder);
        //        return ApiResponse<OrderDto>.SuccessResponse(orderDto, "Order created successfully");
        //    }
        //    catch (Exception ex)
        //    {
        //        await _unitOfWork.RollbackTransactionAsync();
        //        _logger.LogError(ex, "Error creating order");
        //        return ApiResponse<OrderDto>.ErrorResponse("Failed to create order");
        //    }
        //}

        public async Task<ApiResponse<OrderDto>> UpdateOrderStatusAsync(int id, OrderStatus status)
        {
            try
            {
                var order = await _unitOfWork.Orders.GetByIdAsync(id);
                if (order == null)
                {
                    return ApiResponse<OrderDto>.ErrorResponse("Order not found");
                }

                order.Status = status;
                order.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.Orders.UpdateAsync(order);
                await _unitOfWork.SaveChangesAsync();

                var orderDto = _mapper.Map<OrderDto>(order);
                return ApiResponse<OrderDto>.SuccessResponse(orderDto, "Order status updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status");
                return ApiResponse<OrderDto>.ErrorResponse("Failed to update order status");
            }
        }

        public async Task<ApiResponse<bool>> CancelOrderAsync(int id)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var order = await _unitOfWork.Orders.GetByIdAsync(id);
                if (order == null)
                {
                    return ApiResponse<bool>.ErrorResponse("Order not found");
                }

                if (order.Status == OrderStatus.Delivered)
                {
                    return ApiResponse<bool>.ErrorResponse("Cannot cancel delivered order");
                }

                var orderItems = await _unitOfWork.OrderItems.FindAsync(oi => oi.OrderId == id);

                foreach (var item in orderItems)
                {
                    var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
                    if (product != null)
                    {
                        product.StockQuantity += item.Quantity;
                        await _unitOfWork.Products.UpdateAsync(product);
                    }
                }

                order.Status = OrderStatus.Cancelled;
                order.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.Orders.UpdateAsync(order);
                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitTransactionAsync();

                return ApiResponse<bool>.SuccessResponse(true, "Order cancelled successfully");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error cancelling order");
                return ApiResponse<bool>.ErrorResponse("Failed to cancel order");
            }
        }

        private string GenerateOrderNumber()
        {
            return $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}-{new Random().Next(1000, 9999)}";
        }
    }
}
