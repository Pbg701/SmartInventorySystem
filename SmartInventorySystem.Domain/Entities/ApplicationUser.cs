using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity;
namespace SmartInventorySystem.Domain.Entities
{
    public class ApplicationUser : IdentityUser<int>
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        public UserRole Role { get; set; } = UserRole.User;

        // Navigation properties
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

        public string FullName => $"{FirstName} {LastName}";
    }
}
