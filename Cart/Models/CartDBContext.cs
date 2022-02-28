using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CartsApi.Models
{
    public class CartDBContext : DbContext
    {
    public CartDBContext(DbContextOptions<CartDBContext> options)
        : base(options)
    {
    }

    public DbSet<Cart> Carts { get; set; }
    }
}
