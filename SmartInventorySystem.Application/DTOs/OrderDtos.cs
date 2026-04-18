using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventorySystem.Application.DTOs
{
    public class OrderDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public List<OrderItemDto> Items { get; set; } = new();
    }

    public class OrderItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal { get; set; }
    }

    public class CreateOrderDto
    {
        public List<CreateOrderItemDto> Items { get; set; } = new();
    }

    public class CreateOrderItemDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
