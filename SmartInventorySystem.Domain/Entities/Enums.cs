using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventorySystem.Domain.Entities
{

    public enum OrderStatus
    {
        Pending = 0,
        Confirmed = 1,
        Processing = 2,
        Shipped = 3,
        Delivered = 4,
        Cancelled = 5
    }

    public enum UserRole
    {
        User = 0,
        Manager = 1,
        Admin = 2
    }
}
