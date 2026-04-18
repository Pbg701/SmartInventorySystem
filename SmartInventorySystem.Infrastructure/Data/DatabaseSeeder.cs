using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SmartInventorySystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
namespace SmartInventorySystem.Infrastructure.Data
{

    public class DatabaseSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<DatabaseSeeder>>();

            try
            {
                await context.Database.EnsureCreatedAsync();

                // Seed Roles
                if (!await roleManager.RoleExistsAsync(UserRole.Admin.ToString()))
                    await roleManager.CreateAsync(new IdentityRole<int>(UserRole.Admin.ToString()));

                if (!await roleManager.RoleExistsAsync(UserRole.Manager.ToString()))
                    await roleManager.CreateAsync(new IdentityRole<int>(UserRole.Manager.ToString()));

                if (!await roleManager.RoleExistsAsync(UserRole.User.ToString()))
                    await roleManager.CreateAsync(new IdentityRole<int>(UserRole.User.ToString()));

                // Seed Users
                if (!await userManager.Users.AnyAsync())
                {
                    var adminUser = new ApplicationUser
                    {
                        UserName = "admin@inventory.com",
                        Email = "admin@inventory.com",
                        FirstName = "Admin",
                        LastName = "User",
                        Role = UserRole.Admin,
                        EmailConfirmed = true
                    };
                    await userManager.CreateAsync(adminUser, "Admin@123");
                    await userManager.AddToRoleAsync(adminUser, UserRole.Admin.ToString());

                    var managerUser = new ApplicationUser
                    {
                        UserName = "manager@inventory.com",
                        Email = "manager@inventory.com",
                        FirstName = "Manager",
                        LastName = "User",
                        Role = UserRole.Manager,
                        EmailConfirmed = true
                    };
                    await userManager.CreateAsync(managerUser, "Manager@123");
                    await userManager.AddToRoleAsync(managerUser, UserRole.Manager.ToString());

                    var regularUser = new ApplicationUser
                    {
                        UserName = "user@inventory.com",
                        Email = "user@inventory.com",
                        FirstName = "Regular",
                        LastName = "User",
                        Role = UserRole.User,
                        EmailConfirmed = true
                    };
                    await userManager.CreateAsync(regularUser, "User@123");
                    await userManager.AddToRoleAsync(regularUser, UserRole.User.ToString());
                }

                // Seed Suppliers
                if (!context.Suppliers.Any())
                {
                    var suppliers = new List<Supplier>
                {
                    new() { Name = "Tech Distributors Inc.", Email = "sales@techdist.com", Phone = "555-0101", Address = "123 Tech Street, Silicon Valley, CA" },
                    new() { Name = "Global Electronics Ltd.", Email = "info@globalelec.com", Phone = "555-0102", Address = "456 Global Road, New York, NY" },
                    new() { Name = "Quality Components Co.", Email = "orders@qualitycomp.com", Phone = "555-0103", Address = "789 Quality Ave, Austin, TX" },
                    new() { Name = "Premier Supplies LLC", Email = "contact@premiersupplies.com", Phone = "555-0104", Address = "321 Premier Blvd, Chicago, IL" }
                };
                    await context.Suppliers.AddRangeAsync(suppliers);
                    await context.SaveChangesAsync();
                }

                // Seed Products
                if (!context.Products.Any())
                {
                    var suppliers = await context.Suppliers.ToListAsync();
                    var products = new List<Product>
                {
                    new() { Name = "Laptop Pro X1", Sku = "LAP-001", Description = "High-performance laptop for professionals", Price = 1299.99m, StockQuantity = 5, MinimumStockThreshold = 10, SupplierId = suppliers[0].Id },
                    new() { Name = "Wireless Mouse M3", Sku = "MOU-002", Description = "Ergonomic wireless mouse", Price = 29.99m, StockQuantity = 3, MinimumStockThreshold = 20, SupplierId = suppliers[0].Id },
                    new() { Name = "Mechanical Keyboard K7", Sku = "KEY-003", Description = "RGB mechanical keyboard", Price = 89.99m, StockQuantity = 15, MinimumStockThreshold = 10, SupplierId = suppliers[0].Id },
                    new() { Name = "4K Monitor U32", Sku = "MON-004", Description = "32-inch 4K UHD monitor", Price = 399.99m, StockQuantity = 2, MinimumStockThreshold = 5, SupplierId = suppliers[1].Id },
                    new() { Name = "External SSD 1TB", Sku = "SSD-005", Description = "Portable 1TB external SSD", Price = 129.99m, StockQuantity = 8, MinimumStockThreshold = 10, SupplierId = suppliers[1].Id },
                    new() { Name = "USB-C Hub 7-in-1", Sku = "HUB-006", Description = "Multi-port USB-C hub", Price = 49.99m, StockQuantity = 1, MinimumStockThreshold = 15, SupplierId = suppliers[1].Id },
                    new() { Name = "Webcam HD Pro", Sku = "CAM-007", Description = "1080p HD webcam", Price = 79.99m, StockQuantity = 4, MinimumStockThreshold = 8, SupplierId = suppliers[2].Id },
                    new() { Name = "Noise Cancelling Headphones", Sku = "AUD-008", Description = "Wireless noise-cancelling headphones", Price = 199.99m, StockQuantity = 12, MinimumStockThreshold = 10, SupplierId = suppliers[2].Id },
                    new() { Name = "Smartphone Stand", Sku = "ACC-009", Description = "Adjustable smartphone stand", Price = 19.99m, StockQuantity = 25, MinimumStockThreshold = 20, SupplierId = suppliers[2].Id },
                    new() { Name = "Gaming Chair", Sku = "CHA-010", Description = "Ergonomic gaming chair", Price = 299.99m, StockQuantity = 6, MinimumStockThreshold = 5, SupplierId = suppliers[3].Id },
                    new() { Name = "Desk Lamp LED", Sku = "LMP-011", Description = "Adjustable LED desk lamp", Price = 39.99m, StockQuantity = 0, MinimumStockThreshold = 5, SupplierId = suppliers[3].Id }
                };
                    await context.Products.AddRangeAsync(products);
                    await context.SaveChangesAsync();
                }

                // Seed Orders
                if (!context.Orders.Any())
                {
                    var user = await userManager.FindByEmailAsync("user@inventory.com");
                    var products = await context.Products.ToListAsync();

                    if (user != null && products.Any())
                    {
                        var orders = new List<Order>
                    {
                        new()
                        {
                            OrderNumber = "ORD-001",
                            OrderDate = DateTime.UtcNow.AddDays(-5),
                            UserId = user.Id,
                            Status = OrderStatus.Delivered,
                            TotalAmount = products[0].Price * 1 + products[1].Price * 2
                        },
                        new()
                        {
                            OrderNumber = "ORD-002",
                            OrderDate = DateTime.UtcNow.AddDays(-2),
                            UserId = user.Id,
                            Status = OrderStatus.Shipped,
                            TotalAmount = products[2].Price * 1 + products[3].Price * 1
                        }
                    };
                        await context.Orders.AddRangeAsync(orders);
                        await context.SaveChangesAsync();

                        // Add Order Items
                        var orderItems = new List<OrderItem>
                    {
                        new() { OrderId = orders[0].Id, ProductId = products[0].Id, Quantity = 1, UnitPrice = products[0].Price },
                        new() { OrderId = orders[0].Id, ProductId = products[1].Id, Quantity = 2, UnitPrice = products[1].Price },
                        new() { OrderId = orders[1].Id, ProductId = products[2].Id, Quantity = 1, UnitPrice = products[2].Price },
                        new() { OrderId = orders[1].Id, ProductId = products[3].Id, Quantity = 1, UnitPrice = products[3].Price }
                    };
                        await context.OrderItems.AddRangeAsync(orderItems);
                        await context.SaveChangesAsync();
                    }
                }

                logger.LogInformation("Database seeding completed successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the database");
            }
        }
    }
}
