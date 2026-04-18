using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventorySystem.Domain.Entities
{
    public class Order : BaseEntity
    {
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public int UserId { get; set; }

        // Navigation properties
        public virtual ApplicationUser? User { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
