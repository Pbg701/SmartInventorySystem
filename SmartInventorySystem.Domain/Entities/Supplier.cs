using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventorySystem.Domain.Entities
{
    public class Supplier : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;

        // Navigation properties
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
