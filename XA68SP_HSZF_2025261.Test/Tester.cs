using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XA68SP_HSZF_2025261.Application;
using XA68SP_HSZF_2025261.Models;
using XA68SP_HSZF_2025261.Persistence.MsSql;

namespace XA68SP_HSZF_2025261.Test
{
    public class Tester
    {
        private Mock<ICustomerDataProvider> mockCustomerDataProvider;
        private Mock<IProductDataProvider> mockProductDataProvider;
        private Mock<IOrderDataProvider> mockOrderDataProvider;

        private ICustomerService customerService;
        private IProductService productService;
        private IOrderService orderService;

        public Tester()
        {
            mockCustomerDataProvider = new Mock<ICustomerDataProvider>();
            mockProductDataProvider = new Mock<IProductDataProvider>();
            mockOrderDataProvider = new Mock<IOrderDataProvider>();

            customerService = new CustomerService(mockCustomerDataProvider.Object, mockProductDataProvider.Object, mockOrderDataProvider.Object);
            productService = new ProductService(mockProductDataProvider.Object, mockOrderDataProvider.Object);
            orderService = new OrderService(mockOrderDataProvider.Object, mockProductDataProvider.Object, productService);

            var customers = new List<Customer>
        {
            new Customer { Id = 10, Name = "Michael Scott", Email = "michael.scott@dundermifflin.com", PhoneNumber = "+11111111" },
            new Customer { Id = 11, Name = "Pam Beesly", Email = "pam.beesly@dundermifflin.com", PhoneNumber = "+22222222" }
        };

            var products = new List<Product>
        {
            new Product { Id = 100, Name = "iPhone 14 Pro", Price = 520000, Stock = 30, Category = "Smartphones", Description = "Apple flagship phone" },
            new Product { Id = 101, Name = "Razer BlackWidow Keyboard", Price = 60000, Stock = 40, Category = "Accessories", Description = "Mechanical gaming keyboard" }
        };

            var orders = new List<Order>
        {
            new Order { Id = 500, ProductId = 100, CustomerId = 10, Quantity = 1, OrderDate = DateTime.Now.AddDays(-20) },
            new Order { Id = 501, ProductId = 101, CustomerId = 11, Quantity = 3, OrderDate = DateTime.Now.AddDays(-7) }
        };

            mockCustomerDataProvider.Setup(repo => repo.ReadAllCustomers()).Returns(customers.AsQueryable());
            mockProductDataProvider.Setup(repo => repo.ReadAllProducts()).Returns(products.AsQueryable());
            mockOrderDataProvider.Setup(repo => repo.ReadAllOrders()).Returns(orders.AsQueryable());
        }

        [Test]
        public void TestGetCustomers()
        {
            var customers = customerService.GetAllCustomers();

            Assert.That(customers.Count(), Is.EqualTo(2));
            Assert.That(customers.First().Name, Is.EqualTo("Michael Scott"));
        }

        [Test]
        public void TestCreateCustomer()
        {
            var newCustomer = new Customer
            {
                Name = "Dwight Schrute",
                Email = "dwight.schrute@dundermifflin.com",
                PhoneNumber = "+33333333"
            };

            customerService.CreateCustomer(newCustomer);

            mockCustomerDataProvider.Verify(repo => repo.CreateCustomer(It.Is<Customer>(c =>
                c.Name == "Dwight Schrute" &&
                c.Email == "dwight.schrute@dundermifflin.com" &&
                c.PhoneNumber == "+33333333")), Times.Once);
        }

        [Test]
        public void TestGetProducts()
        {
            var products = productService.GetAllProducts();

            Assert.That(products.Count(), Is.EqualTo(2));
            Assert.That(products.First().Name, Is.EqualTo("iPhone 14 Pro"));
        }

        [Test]
        public void TestCreateProduct()
        {
            var newProduct = new Product
            {
                Name = "MacBook Air M3",
                Price = 650000,
                Stock = 10,
                Category = "Laptops",
                Description = "Ultra-thin Apple laptop"
            };

            productService.CreateProduct(newProduct);

            mockProductDataProvider.Verify(repo => repo.CreateProduct(It.Is<Product>(p =>
                p.Name == "MacBook Air M3" &&
                p.Price == 650000 &&
                p.Stock == 10)), Times.Once);
        }

        [Test]
        public void TestGetOrders()
        {
            var orders = orderService.GetAllOrders();

            Assert.That(orders.Count(), Is.EqualTo(2));
            Assert.That(orders.First().ProductId, Is.EqualTo(100));
        }

        [Test]
        public void TestCreateOrder()
        {
            var newOrder = new Order
            {
                ProductId = 100,
                CustomerId = 11,
                Quantity = 5,
                OrderDate = DateTime.Now
            };

            orderService.CreateOrder(newOrder);

            mockOrderDataProvider.Verify(repo => repo.CreateOrder(It.Is<Order>(o =>
                o.ProductId == 100 &&
                o.CustomerId == 11 &&
                o.Quantity == 5)), Times.Once);
        }


        [Test]
        public void TestDeleteCustomer()
        {
            int customerIdToDelete = 10;

            customerService.DeleteCustomer(customerIdToDelete);

            mockCustomerDataProvider.Verify(repo =>
                repo.DeleteCustomer(It.Is<int>(id => id == customerIdToDelete)),
                Times.Once);
        }
        [Test]
        public void TestGetCustomerById_UsingGetAll()
        {
            int customerId = 11;

            var customer = customerService
                .GetAllCustomers()
                .FirstOrDefault(c => c.Id == customerId);

            Assert.That(customer, Is.Not.Null);
            Assert.That(customer.Name, Is.EqualTo("Pam Beesly"));
            Assert.That(customer.Email, Is.EqualTo("pam.beesly@dundermifflin.com"));
        }

        [Test]
        public void TestGetProductById_UsingGetAll()
        {
            int productId = 101;

            var product = productService
                .GetAllProducts()
                .FirstOrDefault(p => p.Id == productId);

            Assert.That(product, Is.Not.Null);
            Assert.That(product.Name, Is.EqualTo("Razer BlackWidow Keyboard"));
            Assert.That(product.Price, Is.EqualTo(60000));
        }

        [Test]
        public void TestGetOrdersByCustomerId_UsingGetAll()
        {
            int customerId = 10;

            var orders = orderService
                .GetAllOrders()
                .Where(o => o.CustomerId == customerId);

            Assert.That(orders.Count(), Is.EqualTo(1));
            Assert.That(orders.First().ProductId, Is.EqualTo(100));
        }

        [Test]
        public void TestGetOrdersByProductId_UsingGetAll()
        {
            int productId = 101;

            var orders = orderService
                .GetAllOrders()
                .Where(o => o.ProductId == productId);

            Assert.That(orders.Count(), Is.EqualTo(1));
            Assert.That(orders.First().CustomerId, Is.EqualTo(11));
        }

        [Test]
        public void TestUpdateCustomer_UsingUpdateMethod()
        {
            var updatedCustomer = new Customer
            {
                Id = 10,
                Name = "Michael Gary Scott",
                Email = "michael.g.scott@dundermifflin.com",
                PhoneNumber = "+11111111"
            };

            customerService.UpdateCustomer(updatedCustomer);

            mockCustomerDataProvider.Verify(repo => repo.UpdateCustomer(It.Is<Customer>(c =>
                c.Id == 10 &&
                c.Name == "Michael Gary Scott" &&
                c.Email == "michael.g.scott@dundermifflin.com" &&
                c.PhoneNumber == "+11111111"
            )), Times.Once);
        }

        [Test]
        public void TestDeleteProduct()
        {
            int productIdToDelete = 100;

            productService.DeleteProduct(productIdToDelete);

            mockProductDataProvider.Verify(repo =>
                repo.DeleteProduct(It.Is<int>(id => id == productIdToDelete)),
                Times.Once);
        }

        [Test]
        public void TestDeleteOrder()
        {
            int orderIdToDelete = 500;

            orderService.DeleteOrder(orderIdToDelete);

            mockOrderDataProvider.Verify(repo =>
                repo.DeleteOrder(It.Is<int>(id => id == orderIdToDelete)),
                Times.Once);
        }

        [Test]
        public void TestUpdateProduct()
        {
            var updatedProduct = new Product
            {
                Id = 100,
                Name = "iPhone 14 Pro Max",
                Price = 600000,
                Stock = 25,
                Category = "Smartphones",
                Description = "Larger Apple flagship model"
            };

            productService.UpdateProduct(updatedProduct);

            mockProductDataProvider.Verify(repo => repo.UpdateProduct(It.Is<Product>(p =>
                p.Id == 100 &&
                p.Name == "iPhone 14 Pro Max" &&
                p.Price == 600000 &&
                p.Stock == 25 &&
                p.Category == "Smartphones" &&
                p.Description == "Larger Apple flagship model")), Times.Once);
        }

    }

}
