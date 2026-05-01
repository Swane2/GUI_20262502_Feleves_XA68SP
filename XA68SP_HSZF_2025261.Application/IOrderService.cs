using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XA68SP_HSZF_2025261.Models;

namespace XA68SP_HSZF_2025261.Application
{
    public interface IOrderService
    {
        void CreateOrder(Order order);
        void UpdateOrder(Order order);
        void DeleteOrder(int orderId);
        IEnumerable<Order> GetAllOrders();
        List<string> GenerateMonthlySalesReport();
        event EventHandler<string>? OrderDeleted;
    }
}
