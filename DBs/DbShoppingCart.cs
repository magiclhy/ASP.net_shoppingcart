using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ShoppingCart.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShoppingCart.DBs
{
    public class DbShoppingCart : DbContext
    {
        protected IConfiguration configuration;

        public DbShoppingCart(DbContextOptions<DbShoppingCart> options)
            : base(options) { }

        // public DbSet<ActivationCode> ActivationCodes { get; set; } // Tentatively commented out, can be deleted afterwards
        public DbSet<Cart> Carts { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<User> Users { get; set; }

    }
}