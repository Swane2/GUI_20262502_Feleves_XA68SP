using Castle.Core.Resource;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XA68SP_HSZF_2025261.Models;

namespace XA68SP_HSZF_2025261.Persistence.MsSql
{
    public class ShopDbContext : DbContext
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }

        public ShopDbContext()
        {
            Database.EnsureDeleted();
            Database.EnsureCreated();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string connStr =
                @"Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=shopdb;Integrated Security=True;MultipleActiveResultSets=true";
            optionsBuilder.UseSqlServer(connStr);
            base.OnConfiguring(optionsBuilder);
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Order>()
                .HasOne(order => order.Product)
                .WithMany(product => product.Orders)
                .HasForeignKey(order => order.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Order>()
                .HasOne(order => order.Customer)
                .WithMany(customer => customer.Orders)
                .HasForeignKey(order => order.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Product>()
                .Property(product => product.Price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Customer>()
                .Property(customer => customer.Name);

            modelBuilder.Entity<Customer>()
                .Property(customer => customer.Email)
                .HasMaxLength(100);

            modelBuilder.Entity<Customer>()
                .Property(customer => customer.PhoneNumber)
                .HasMaxLength(30);
        }
    }
}
