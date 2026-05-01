using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XA68SP_HSZF_2025261.Models;

namespace XA68SP_HSZF_2025261.Persistence.MsSql
{   
    public class CustomerDataProvider : ICustomerDataProvider
    {
        private readonly ShopDbContext dbContext;
        public CustomerDataProvider(ShopDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public void CreateCustomer(Customer customer)
        {
            dbContext.Customers.Add(customer);
            dbContext.SaveChanges();
        }
        public void DeleteCustomer(int customerId)
        {
            var customerToDelete = dbContext.Customers.FirstOrDefault(c => c.Id == customerId);
            if (customerToDelete != null)
            {
                dbContext.Customers.Remove(customerToDelete);
                dbContext.SaveChanges();
            }
        }
        public IEnumerable<Customer> ReadAllCustomers()
        {
            return dbContext.Customers;
        }
        public void UpdateCustomer(Customer customer)
        {
            var customerToUpdate = dbContext.Customers.FirstOrDefault(c => c.Id == customer.Id);
            if (customerToUpdate != null)
            {
                customerToUpdate.Name = customer.Name;
                customerToUpdate.Email = customer.Email;
                customerToUpdate.PhoneNumber = customer.PhoneNumber;
                dbContext.SaveChanges();
            }
        }
    }
}
