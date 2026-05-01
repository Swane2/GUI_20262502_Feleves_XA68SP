using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XA68SP_HSZF_2025261.Models;
using XA68SP_HSZF_2025261.Persistence.MsSql;

namespace XA68SP_HSZF_2025261.Application
{
    public class OrderService : IOrderService
    {
        private readonly IOrderDataProvider orderDataProvider;
        private readonly IProductDataProvider productDataProvider;
        private readonly IProductService productService;

        public OrderService(IOrderDataProvider orderDataProvider, IProductDataProvider productDataProvider,
            IProductService productService)
        {
            this.orderDataProvider = orderDataProvider;
            this.productService = productService;
            this.productDataProvider = productDataProvider;
            orderDataProvider.OrderDeleted += OnOrderDeletedFromDataProvider;
        }
        public event EventHandler<string>? OrderDeleted;
        private void OnOrderDeletedFromDataProvider(object? sender, string message)
        {
            OrderDeleted?.Invoke(this, message);
        }
        public void CreateOrder(Order order)
        {
            if (order.ProductId <= 0 || order.CustomerId <= 0 || order.Quantity <= 0)
                throw new ArgumentException("Order details are invalid. Ensure all fields are properly set.");

            orderDataProvider.CreateOrder(order);

            var product = productService.GetAllProducts().FirstOrDefault(p => p.Id == order.ProductId);
            if (product != null)
            {
                product.Stock -= order.Quantity;
                productService.UpdateProduct(product);
            }
            else
            {
                throw new InvalidOperationException($"Product with ID {order.ProductId} not found.");
            }
        }

        public void UpdateOrder(Order order)
        {
            var existingOrder = orderDataProvider.ReadAllOrders().FirstOrDefault(o => o.Id == order.Id);

            if (existingOrder == null)
                throw new InvalidOperationException($"Order with ID {order.Id} does not exist.");

            orderDataProvider.UpdateOrder(order);
        }

        public void DeleteOrder(int orderId)
        {
            var existingOrder = orderDataProvider.ReadAllOrders().FirstOrDefault(o => o.Id == orderId);

            if (existingOrder == null)
                throw new InvalidOperationException($"Order with ID {orderId} does not exist.");

            orderDataProvider.DeleteOrder(orderId);
        }

        public IEnumerable<Order> GetAllOrders()
        {
            return orderDataProvider.ReadAllOrders();
        }
        public List<string> GenerateMonthlySalesReport()
        {
            var orders = orderDataProvider.ReadAllOrders();
            var products = productDataProvider.ReadAllProducts();

            var monthlySales = orders
                .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                .Select(group => new
                {
                    Month = $"{group.Key.Year}-{group.Key.Month:00}",
                    TotalSales = group.Sum(o => o.Quantity),
                    TotalRevenue = group.Sum(o => o.Quantity * products.FirstOrDefault(p => p.Id == o.ProductId)?.Price ?? 0)
                })
                .OrderBy(report => report.Month)
                .ToList();

            var reportLines = new List<string> { "Monthly Sales Report:" };
            foreach (var report in monthlySales)
            {
                reportLines.Add($"Month: {report.Month}");
                reportLines.Add($"  Total Sales: {report.TotalSales}");
                reportLines.Add($"  Total Revenue: {report.TotalRevenue:C}");
                reportLines.Add(string.Empty);
            }

            return reportLines;
        }
    }
}
