using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XA68SP_HSZF_2025261.Models;

namespace XA68SP_HSZF_2025261.Persistence.MsSql
{
    public interface IOrderDataProvider
    {
        void CreateOrder(Order order);
        void DeleteOrder(int orderId);
        IEnumerable<Order> ReadAllOrders();
        void UpdateOrder(Order order);
        event EventHandler<string>? OrderDeleted;
    }
}
