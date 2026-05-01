using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XA68SP_HSZF_2025261.Models;

namespace XA68SP_HSZF_2025261.Persistence.MsSql
{   
    public class OrderDataProvider : IOrderDataProvider
    {
        private readonly ShopDbContext dbContext;
        public OrderDataProvider(ShopDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public event EventHandler<string>? OrderDeleted;
        public void CreateOrder(Order order)
        {
            dbContext.Orders.Add(order);
            dbContext.SaveChanges();
        }
        public void DeleteOrder(int orderId)
        {
            var orderToDelete = dbContext.Orders.FirstOrDefault(o => o.Id == orderId);
            if (orderToDelete != null)
            {
                dbContext.Orders.Remove(orderToDelete);
                dbContext.SaveChanges();
                OrderDeleted?.Invoke(this, $"Order ID {orderId} was deleted.");
            }
        }
        public IEnumerable<Order> ReadAllOrders()
        {
            return dbContext.Orders
                .Include(o => o.Product)
                .Include(o => o.Customer)
                .ToList();
        }
        public void UpdateOrder(Order order)
        {
            var orderToUpdate = dbContext.Orders.FirstOrDefault(o => o.Id == order.Id);
            if (orderToUpdate != null)
            {
                orderToUpdate.ProductId = order.ProductId;
                orderToUpdate.CustomerId = order.CustomerId;
                orderToUpdate.Quantity = order.Quantity;
                orderToUpdate.OrderDate = order.OrderDate;
                dbContext.SaveChanges();
            }
        }
    }
}
