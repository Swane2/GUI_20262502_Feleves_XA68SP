using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using XA68SP_HSZF_2025261.Models;
using XA68SP_HSZF_2025261.Persistence.MsSql;

namespace XA68SP_HSZF_2025261.Application
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerDataProvider customerDataProvider;
        private readonly IProductDataProvider productDataProvider;
        private readonly IOrderDataProvider orderDataProvider;
        public CustomerService(ICustomerDataProvider customerDataProvider,
            IProductDataProvider productDataProvider, IOrderDataProvider orderDataProvider)
        {
            this.customerDataProvider = customerDataProvider;
            this.productDataProvider = productDataProvider;
            this.orderDataProvider = orderDataProvider;
        }
        public void ImportFromXml(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"The file {filePath} does not exist.");

            var xmlContent = File.ReadAllText(filePath);
            var xDoc = XDocument.Parse(xmlContent);

            var customers = xDoc.Descendants("Customer");
            foreach (var customerElement in customers)
            {
                var id = int.Parse(customerElement.Element("Id").Value);
                var name = customerElement.Element("Name").Value;
                var email = customerElement.Element("Email").Value;
                var phoneNumber = customerElement.Element("PhoneNumber").Value;

                if (!customerDataProvider.ReadAllCustomers().Any(c => c.Id == id))
                {
                    var customer = new Customer
                    {
                        Id = id,
                        Name = name,
                        Email = email,
                        PhoneNumber = phoneNumber
                    };

                    customerDataProvider.CreateCustomer(customer);
                }
            }

            var products = xDoc.Descendants("Product");
            foreach (var productElement in products)
            {
                var id = int.Parse(productElement.Element("Id").Value);
                var name = productElement.Element("Name").Value;
                var brand = productElement.Element("Brand").Value;
                var category = productElement.Element("Category").Value;
                var price = decimal.Parse(productElement.Element("Price").Value);
                var stock = int.Parse(productElement.Element("Stock").Value);
                var description = productElement.Element("Description").Value;

                var existingProduct = productDataProvider.ReadAllProducts().FirstOrDefault(p => p.Id == id);

                if (existingProduct != null)
                {
                    existingProduct.Stock += stock;
                    productDataProvider.UpdateProduct(existingProduct);
                }
                else
                {
                    var product = new Product
                    {
                        Id = id,
                        Name = name,
                        Brand = brand,
                        Category = category,
                        Price = price,
                        Stock = stock,
                        Description = description
                    };

                    productDataProvider.CreateProduct(product);
                }
            }

            var orders = xDoc.Descendants("Order");
            foreach (var orderElement in orders)
            {
                var id = int.Parse(orderElement.Element("Id").Value);
                var productId = int.Parse(orderElement.Element("ProductId").Value);
                var customerId = int.Parse(orderElement.Element("CustomerId").Value);
                var quantity = int.Parse(orderElement.Element("Quantity").Value);
                var orderDate = DateTime.Parse(orderElement.Element("OrderDate").Value);

                var order = new Order
                {
                    Id = id,
                    ProductId = productId,
                    CustomerId = customerId,
                    Quantity = quantity,
                    OrderDate = orderDate
                };

                orderDataProvider.CreateOrder(order);
            }
        }
        public IEnumerable<Customer> GetAllCustomers()
        {
            return customerDataProvider.ReadAllCustomers();
        }


        public void CreateCustomer(Customer customer)
        {
            customerDataProvider.CreateCustomer(customer);
        }
        public void DeleteCustomer(int customerId)
        {
            customerDataProvider.DeleteCustomer(customerId);
        }
        public void UpdateCustomer(Customer customer)
        {
            customerDataProvider.UpdateCustomer(customer);
        }
        public List<Customer> GetCustomersWithoutOrders()
        {
            var customers = customerDataProvider.ReadAllCustomers();
            var orders = orderDataProvider.ReadAllOrders();

            var customersWithoutOrders = customers
                .Where(c => !orders.Any(o => o.CustomerId == c.Id))
                .ToList();

            return customersWithoutOrders;
        }
    }
}
