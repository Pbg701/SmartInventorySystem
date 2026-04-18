using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventorySystem.Domain.Entities
{
    public class OrderItem : BaseEntity
    {
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal => Quantity * UnitPrice;

        // Navigation properties
        public virtual Order? Order { get; set; }
        public virtual Product? Product { get; set; }
    }
}
