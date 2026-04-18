using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventorySystem.Application.DTOs
{

    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public int MinimumStockThreshold { get; set; }
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public bool IsLowStock { get; set; }
    }

    public class CreateProductDto
    {
        public string Name { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public int MinimumStockThreshold { get; set; }
        public int SupplierId { get; set; }
    }

    public class UpdateProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public int MinimumStockThreshold { get; set; }
        public int SupplierId { get; set; }
    }
}
