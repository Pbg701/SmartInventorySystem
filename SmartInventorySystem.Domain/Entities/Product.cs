using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventorySystem.Domain.Entities
{

    public class Product : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public int MinimumStockThreshold { get; set; }
        public int SupplierId { get; set; }

        // Navigation properties
        public virtual Supplier? Supplier { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        public bool IsLowStock => StockQuantity <= MinimumStockThreshold;
    }
}
