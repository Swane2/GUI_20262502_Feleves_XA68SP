using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XA68SP_HSZF_2025261.Models;

namespace XA68SP_HSZF_2025261.Application
{
    public interface ICustomerService
    {
        void ImportFromXml(string filePath);
        IEnumerable<Customer> GetAllCustomers();
        void CreateCustomer(Customer customer);
        void DeleteCustomer(int customerId);
        void UpdateCustomer(Customer customer);
        List<Customer> GetCustomersWithoutOrders();
    }
}
