using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrdersApi.Models
{
    public class OrderDBContext : DbContext
    {
    public OrderDBContext(DbContextOptions<OrderDBContext> options)
        : base(options)
    {
    }

    public DbSet<Order> Orders { get; set; }
    }
}
