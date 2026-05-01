using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XA68SP_HSZF_2025261.Models;

namespace XA68SP_HSZF_2025261.Persistence.MsSql
{
    public interface ICustomerDataProvider
    {
        void CreateCustomer(Customer customer);
        void DeleteCustomer(int customerId);
        IEnumerable<Customer> ReadAllCustomers();
        void UpdateCustomer(Customer customer);
    }
}
