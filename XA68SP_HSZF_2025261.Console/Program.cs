using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Xml.Linq;
using XA68SP_HSZF_2025261.Application;
using XA68SP_HSZF_2025261.Models;
using XA68SP_HSZF_2025261.Persistence.MsSql;

namespace XA68SP_HSZF_2025261
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddScoped<ShopDbContext>();
                    services.AddSingleton<ICustomerDataProvider, CustomerDataProvider>();
                    services.AddSingleton<IOrderDataProvider, OrderDataProvider>();
                    services.AddSingleton<IProductDataProvider, ProductDataProvider>();
                    services.AddSingleton<ICustomerService, CustomerService>();
                    services.AddSingleton<IOrderService, OrderService>();
                    services.AddSingleton<IProductService, ProductService>();
                })
                .Build();

            host.Start();
            MainMenu(host);
        }
        static void MainMenu(IHost host)
        {
            bool exit = false;
            ConfigureEvents(host);
            SetupOrderDeletedEvent(host);
            while (!exit)
            {
                Console.WriteLine("\nMain Menu:");
                Console.WriteLine("1. LOAD XML ('shop.xml')");
                Console.WriteLine("2. CUSTOMER BY NAME");
                Console.WriteLine("3. PRODUCTS BY PAGE");
                Console.WriteLine("4. ALL ORDERS");
                Console.WriteLine("5. CUSTOMER CREATING");
                Console.WriteLine("6. PRODUCT CREATING");
                Console.WriteLine("7. ORDER CREATING");
                Console.WriteLine("8. PRODUCT UPDATING");
                Console.WriteLine("9. ORDER UPDATING");
                Console.WriteLine("10. CUSTOMER DELETING");
                Console.WriteLine("11. PRODUCT DELETING");
                Console.WriteLine("12. ORDER DELETING");
                Console.WriteLine("13. CUSTOMERS WITHOUT ORDERS");
                Console.WriteLine("14. REVENUE REPORT");
                Console.WriteLine("15. LOW COST TO JSON");                
                Console.WriteLine("16. MONTHLY SALES REPORT");
                Console.WriteLine("0. EXIT");
                Console.Write(" ");

                var input = Console.ReadLine();
                switch (input)
                {
                    case "1":
                        Console.WriteLine("XML FILE PATH:");
                        var filePath = Console.ReadLine();
                        MarketDataLoader(host, filePath);
                        break;
                    case "2":
                        SearchCustomerByName(host);
                        break;
                    case "3":
                        ProductsByPage(host);
                        break;
                    case "4":
                        ListAllOrders(host);
                        break;
                    case "5":
                        CreateCustomer(host);
                        break;
                    case "6":
                        CreateProduct(host);
                        break;
                    case "7":
                        CreateOrder(host);
                        break;
                    case "8":
                        UpdateProduct(host);
                        break;
                    case "9":
                        UpdateOrder(host);
                        break;
                    case "10":
                        DeleteCustomer(host);
                        break;
                    case "11":
                        DeleteProduct(host);
                        break;
                    case "12":
                        DeleteOrder(host);
                        break;
                    case "13":
                        CustomersWithoutOrders(host);                        
                        break;
                    case "14":
                        CategoryRevenueReports(host);
                        break;
                    case "15":
                        LowStockProductsToJson(host);
                        break;
                    case "16":
                        MonthlySalesReport(host);
                        break;
                    case "0":
                        exit = true;
                        break;
                    default:
                        Console.WriteLine(" ");
                        break;
                }
            }
        }
        static void MarketDataLoader(IHost host, string filePath)
        {
            Console.WriteLine($"Loading data '{filePath}'...");

            if (!File.Exists(filePath))
            {
                Console.WriteLine("File does not exist.");
                return;
            }

            using (var scope = host.Services.CreateScope())
            {
                var customerService = scope.ServiceProvider.GetRequiredService<ICustomerService>();
                var productService = scope.ServiceProvider.GetRequiredService<IProductService>();
                var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();

                var xmlContent = File.ReadAllText(filePath);
                var xDoc = XDocument.Parse(xmlContent);

                var customers = xDoc.Descendants("Customer");
                foreach (var customerElement in customers)
                {
                    var customer = new Customer
                    {
                        Id = int.Parse(customerElement.Element("Id").Value),
                        Name = customerElement.Element("Name").Value,
                        Email = customerElement.Element("Email").Value,
                        PhoneNumber = customerElement.Element("PhoneNumber").Value
                    };

                    var existingCustomers = customerService.GetAllCustomers();
                    if (!existingCustomers.Any(c => c.Id == customer.Id))
                    {
                        customerService.CreateCustomer(customer);
                    }
                }

                var products = xDoc.Descendants("Product");
                foreach (var productElement in products)
                {
                    var product = new Product
                    {
                        Id = int.Parse(productElement.Element("Id").Value),
                        Name = productElement.Element("Name").Value,
                        Brand = productElement.Element("Brand").Value,
                        Category = productElement.Element("Category").Value,
                        Price = decimal.Parse(productElement.Element("Price").Value),
                        Stock = int.Parse(productElement.Element("Stock").Value),
                        Description = productElement.Element("Description").Value
                    };

                    var existingProducts = productService.GetAllProducts();
                    var existingProduct = existingProducts.FirstOrDefault(p => p.Id == product.Id);
                    if (existingProduct != null)
                    {
                        existingProduct.Stock += product.Stock;
                        productService.UpdateProduct(existingProduct);
                    }
                    else
                    {
                        productService.CreateProduct(product);
                    }
                }

                var orders = xDoc.Descendants("Order");
                foreach (var orderElement in orders)
                {
                    var order = new Order
                    {
                        Id = int.Parse(orderElement.Element("Id").Value),
                        ProductId = int.Parse(orderElement.Element("ProductId").Value),
                        CustomerId = int.Parse(orderElement.Element("CustomerId").Value),
                        Quantity = int.Parse(orderElement.Element("Quantity").Value),
                        OrderDate = DateTime.Parse(orderElement.Element("OrderDate").Value)
                    };

                    orderService.CreateOrder(order);
                }

                Console.WriteLine("Data loaded.");
            }
        }        
        static void ProductsByPage(IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var productService = scope.ServiceProvider.GetRequiredService<IProductService>();

                var products = productService.GetAllProducts().ToList();
                if (!products.Any())
                {
                    Console.WriteLine("No products.");
                    return;
                }

                const int pageSize = 10;
                int currentPage = 0;
                int totalPages = (int)Math.Ceiling((double)products.Count / pageSize);

                while (true)
                {
                    Console.Clear();
                    Console.WriteLine($"Product List - Page {currentPage + 1}/{totalPages}");
                    Console.WriteLine("--------------------------------------------------");

                    var paginatedProducts = products
                        .Skip(currentPage * pageSize)
                        .Take(pageSize);

                    foreach (var product in paginatedProducts)
                    {
                        Console.WriteLine($"ID: {product.Id}");
                        Console.WriteLine($"Name: {product.Name}");
                        Console.WriteLine($"Brand: {product.Brand}");
                        Console.WriteLine($"Category: {product.Category}");
                        Console.WriteLine($"Price: {product.Price:C}");
                        Console.WriteLine($"Stock: {product.Stock}");
                        Console.WriteLine($"Description: {product.Description}");
                        Console.WriteLine("--------------------------------------------------");
                    }

                    Console.WriteLine("Options:");
                    if (currentPage > 0) Console.WriteLine("P - Previous");
                    if (currentPage < totalPages - 1) Console.WriteLine("N - Next");
                    Console.WriteLine("E - Exit");

                    var key = Console.ReadKey(true).Key;
                    if (key == ConsoleKey.P && currentPage > 0)
                    {
                        currentPage--;
                    }
                    else if (key == ConsoleKey.N && currentPage < totalPages - 1)
                    {
                        currentPage++;
                    }
                    else if (key == ConsoleKey.E)
                    {
                        break;
                    }
                }
            }
        }
        static void ListAllOrders(IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();

                var orders = orderService.GetAllOrders();

                Console.WriteLine("\nOrders List:");
                Console.WriteLine();

                if (!orders.Any())
                {
                    Console.WriteLine("No orders.");
                    return;
                }

                foreach (var order in orders)
                {
                    Console.WriteLine($"Order ID: {order.Id}");
                    Console.WriteLine($"Product ID: {order.ProductId}");
                    Console.WriteLine($"Customer ID: {order.CustomerId}");
                    Console.WriteLine($"Quantity: {order.Quantity}");
                    Console.WriteLine($"Order Date: {order.OrderDate:yyyy-MM-dd}");
                    Console.WriteLine();
                }
            }
        }
        static void MonthlySalesReport(IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();
                var report = orderService.GenerateMonthlySalesReport();

                Console.WriteLine(string.Join(Environment.NewLine, report));
            }
        }
        static void CategoryRevenueReports(IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var productService = scope.ServiceProvider.GetRequiredService<IProductService>();
                productService.GenerateCategoryRevenueReports();
            }
        }
        static void LowStockProductsToJson(IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var productService = scope.ServiceProvider.GetRequiredService<IProductService>();
                productService.ExportLowStockProductsToJson();
            }
        }
        static void CustomersWithoutOrders(IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var customerService = scope.ServiceProvider.GetRequiredService<ICustomerService>();
                var customers = customerService.GetCustomersWithoutOrders();

                Console.WriteLine("\nCustomers Without Orders:");
                Console.WriteLine("-------------------------");

                if (!customers.Any())
                {
                    Console.WriteLine("All customers have orders.");
                    return;
                }

                foreach (var customer in customers)
                {
                    Console.WriteLine($"ID: {customer.Id}");
                    Console.WriteLine($"Name: {customer.Name}");
                    Console.WriteLine($"Email: {customer.Email}");
                    Console.WriteLine($"Phone Number: {customer.PhoneNumber}");
                    Console.WriteLine("-------------------------");
                }
            }
        }
        static void CreateCustomer(IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var customerService = scope.ServiceProvider.GetRequiredService<ICustomerService>();

                Console.WriteLine("\nNew Customer");
                Console.WriteLine("----------------------");

                Console.Write("Customer Name: ");
                string name = Console.ReadLine();

                Console.Write("Customer Email: ");
                string email = Console.ReadLine();

                Console.Write("Customer Phone Number: ");
                string phoneNumber = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(phoneNumber))
                {
                    Console.WriteLine("All fields are required.");
                    return;
                }

                var existingCustomers = customerService.GetAllCustomers();
                int nextId = existingCustomers.Any() ? existingCustomers.Max(c => c.Id) + 1 : 1;

                var newCustomer = new Customer
                {
                    Id = nextId,
                    Name = name,
                    Email = email,
                    PhoneNumber = phoneNumber
                };

                try
                {
                    customerService.CreateCustomer(newCustomer);
                    Console.WriteLine("Customer created!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating: {ex.Message}");
                }
            }
        }
        static void CreateProduct(IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var productService = scope.ServiceProvider.GetRequiredService<IProductService>();

                Console.WriteLine("\nNew Product");
                Console.WriteLine("---------------------");

                Console.Write("Product Name: ");
                string name = Console.ReadLine();

                Console.Write("Product Brand: ");
                string brand = Console.ReadLine();

                Console.Write("Product Category: ");
                string category = Console.ReadLine();

                Console.Write("Product Price: ");
                decimal price;
                while (!decimal.TryParse(Console.ReadLine(), out price) || price <= 0)
                {
                    Console.WriteLine("Enter a positive number.");
                }

                Console.Write("Product Stock: ");
                int stock;
                while (!int.TryParse(Console.ReadLine(), out stock) || stock < 0)
                {
                    Console.WriteLine("Enter a non-negative number.");
                }

                Console.Write("Product Description: ");
                string description = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(brand) || string.IsNullOrWhiteSpace(category))
                {
                    Console.WriteLine("Name, brand, and category are required.");
                    return;
                }

                var existingProducts = productService.GetAllProducts();
                int nextId = existingProducts.Any() ? existingProducts.Max(p => p.Id) + 1 : 1;

                var newProduct = new Product
                {
                    Id = nextId,
                    Name = name,
                    Brand = brand,
                    Category = category,
                    Price = price,
                    Stock = stock,
                    Description = description
                };

                try
                {
                    productService.CreateProduct(newProduct);
                    Console.WriteLine("Product created!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating: {ex.Message}");
                }
            }
        }
        static void CreateOrder(IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();
                var productService = scope.ServiceProvider.GetRequiredService<IProductService>();
                var customerService = scope.ServiceProvider.GetRequiredService<ICustomerService>();

                Console.WriteLine("\nNew Order");
                Console.WriteLine("-------------------");

                Console.Write("Product ID: ");
                int productId;
                while (!int.TryParse(Console.ReadLine(), out productId) || !productService.GetAllProducts().Any(p => p.Id == productId))
                {
                    Console.WriteLine("Enter a valid ID.");
                }

                Console.Write("Customer ID: ");
                int customerId;
                while (!int.TryParse(Console.ReadLine(), out customerId) || !customerService.GetAllCustomers().Any(c => c.Id == customerId))
                {
                    Console.WriteLine("Enter a valid ID.");
                }

                Console.Write("Quantity: ");
                int quantity;
                while (!int.TryParse(Console.ReadLine(), out quantity) || quantity <= 0)
                {
                    Console.WriteLine("Enter a positive number.");
                }

                Console.Write("Order Date (yyyy-MM-dd): ");
                DateTime orderDate;
                while (!DateTime.TryParse(Console.ReadLine(), out orderDate))
                {
                    Console.WriteLine("Please use yyyy-MM-dd.");
                }

                var existingOrders = orderService.GetAllOrders();
                int nextId = existingOrders.Any() ? existingOrders.Max(o => o.Id) + 1 : 1;

                var newOrder = new Order
                {
                    Id = nextId,
                    ProductId = productId,
                    CustomerId = customerId,
                    Quantity = quantity,
                    OrderDate = orderDate
                };

                try
                {
                    orderService.CreateOrder(newOrder);
                    Console.WriteLine("Order created!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating: {ex.Message}");
                }
            }
        }
        static void ConfigureEvents(IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var productService = scope.ServiceProvider.GetRequiredService<IProductService>();
                productService.ProductStockLow += OnProductStockLow;
            }
        }
        static void OnProductStockLow(object sender, string message)
        {
            Console.WriteLine($"Warning: {message}");
        }
        static void SetupOrderDeletedEvent(IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();
                orderService.OrderDeleted += OnOrderDeleted;
            }
        }
        static void OnOrderDeleted(object? sender, string message)
        {
            Console.WriteLine($"Event: {message}");
        }
        static void UpdateProduct(IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var productService = scope.ServiceProvider.GetRequiredService<IProductService>();

                Console.WriteLine("\nUpdate an Existing Product");
                Console.WriteLine("--------------------------");

                Console.Write("Product ID to Update: ");
                int productId;
                while (!int.TryParse(Console.ReadLine(), out productId) || !productService.GetAllProducts().Any(p => p.Id == productId))
                {
                    Console.WriteLine("Enter a valid ID.");
                }

                var product = productService.GetAllProducts().FirstOrDefault(p => p.Id == productId);
                if (product == null)
                {
                    Console.WriteLine("Product not found.");
                    return;
                }

                Console.WriteLine($"Updating Product: {product.Name}");
                Console.WriteLine("Press Enter to keep the current value.");

                Console.Write($"Current Name ({product.Name}): ");
                string newName = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(newName))
                {
                    product.Name = newName;
                }

                Console.Write($"Current Brand ({product.Brand}): ");
                string newBrand = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(newBrand))
                {
                    product.Brand = newBrand;
                }

                Console.Write($"Current Category ({product.Category}): ");
                string newCategory = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(newCategory))
                {
                    product.Category = newCategory;
                }

                Console.Write($"Current Price ({product.Price}): ");
                if (decimal.TryParse(Console.ReadLine(), out decimal newPrice) && newPrice > 0)
                {
                    product.Price = newPrice;
                }

                Console.Write($"Current Stock ({product.Stock}): ");
                if (int.TryParse(Console.ReadLine(), out int newStock) && newStock >= 0)
                {
                    product.Stock = newStock;
                }

                Console.Write($"Current Description ({product.Description}): ");
                string newDescription = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(newDescription))
                {
                    product.Description = newDescription;
                }

                try
                {
                    productService.UpdateProduct(product);
                    Console.WriteLine("Product updated!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error updating product: {ex.Message}");
                }
            }
        }
        static void UpdateOrder(IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();
                var productService = scope.ServiceProvider.GetRequiredService<IProductService>();
                var customerService = scope.ServiceProvider.GetRequiredService<ICustomerService>();

                Console.WriteLine("\nUpdate an Existing Order");
                Console.WriteLine("--------------------------");

                Console.Write("Order ID to Update: ");
                int orderId;
                while (!int.TryParse(Console.ReadLine(), out orderId) || !orderService.GetAllOrders().Any(o => o.Id == orderId))
                {
                    Console.WriteLine("Enter a valid ID.");
                }

                var order = orderService.GetAllOrders().FirstOrDefault(o => o.Id == orderId);
                if (order == null)
                {
                    Console.WriteLine("Order not found.");
                    return;
                }

                Console.WriteLine($"Updating Order ID: {order.Id}");
                Console.WriteLine("Press Enter to keep the current value.");

                Console.Write($"Current Product ID ({order.ProductId}): ");
                if (int.TryParse(Console.ReadLine(), out int newProductId) && productService.GetAllProducts().Any(p => p.Id == newProductId))
                {
                    order.ProductId = newProductId;
                }

                Console.Write($"Current Customer ID ({order.CustomerId}): ");
                if (int.TryParse(Console.ReadLine(), out int newCustomerId) && customerService.GetAllCustomers().Any(c => c.Id == newCustomerId))
                {
                    order.CustomerId = newCustomerId;
                }

                Console.Write($"Current Quantity ({order.Quantity}): ");
                if (int.TryParse(Console.ReadLine(), out int newQuantity) && newQuantity > 0)
                {
                    order.Quantity = newQuantity;
                }

                Console.Write($"Current Order Date ({order.OrderDate:yyyy-MM-dd}): ");
                if (DateTime.TryParse(Console.ReadLine(), out DateTime newOrderDate))
                {
                    order.OrderDate = newOrderDate;
                }

                try
                {
                    orderService.UpdateOrder(order);
                    Console.WriteLine("Order updated successfully!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error updating order: {ex.Message}");
                }
            }
        }
        static void DeleteCustomer(IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var customerService = scope.ServiceProvider.GetRequiredService<ICustomerService>();

                Console.WriteLine("\nDelete an Existing Customer");
                Console.WriteLine("----------------------------");

                Console.Write("Customer ID to Delete: ");
                int customerId;
                while (!int.TryParse(Console.ReadLine(), out customerId) || !customerService.GetAllCustomers().Any(c => c.Id == customerId))
                {
                    Console.WriteLine("Enter a valid ID.");
                }

                var customer = customerService.GetAllCustomers().FirstOrDefault(c => c.Id == customerId);
                if (customer == null)
                {
                    Console.WriteLine("Customer not found.");
                    return;
                }

                Console.WriteLine($"Are you sure you want to delete Customer '{customer.Name}' (ID: {customer.Id})? (yes/no)");
                string confirmation = Console.ReadLine()?.ToLower();
                if (confirmation == "yes")
                {
                    try
                    {
                        customerService.DeleteCustomer(customerId);
                        Console.WriteLine("Customer deleted!");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error deleting customer: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine("Customer deletion canceled.");
                }
            }
        }
        static void DeleteProduct(IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var productService = scope.ServiceProvider.GetRequiredService<IProductService>();

                Console.WriteLine("\nDelete an Existing Product");
                Console.WriteLine("---------------------------");

                Console.Write("Product ID to Delete: ");
                int productId;
                while (!int.TryParse(Console.ReadLine(), out productId) || !productService.GetAllProducts().Any(p => p.Id == productId))
                {
                    Console.WriteLine("Enter a valid ID.");
                }

                var product = productService.GetAllProducts().FirstOrDefault(p => p.Id == productId);
                if (product == null)
                {
                    Console.WriteLine("Product not found.");
                    return;
                }

                Console.WriteLine($"Are you sure you want to delete Product '{product.Name}' (ID: {product.Id})? (yes/no)");
                string confirmation = Console.ReadLine()?.ToLower();
                if (confirmation == "yes")
                {
                    try
                    {
                        productService.DeleteProduct(productId);
                        Console.WriteLine("Product deleted!");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error deleting product: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine("Product deletion canceled.");
                }
            }
        }
        static void DeleteOrder(IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();

                Console.WriteLine("\nDelete an Existing Order");
                Console.WriteLine("-------------------------");

                Console.Write("Order ID to Delete: ");
                int orderId;
                while (!int.TryParse(Console.ReadLine(), out orderId) || !orderService.GetAllOrders().Any(o => o.Id == orderId))
                {
                    Console.WriteLine("Enter a valid ID.");
                }

                var order = orderService.GetAllOrders().FirstOrDefault(o => o.Id == orderId);
                if (order == null)
                {
                    Console.WriteLine("Order not found.");
                    return;
                }

                Console.WriteLine($"Are you sure you want to delete Order ID: {order.Id} (Product ID: {order.ProductId}, Customer ID: {order.CustomerId})? (yes/no)");
                string confirmation = Console.ReadLine()?.ToLower();
                if (confirmation == "yes")
                {
                    try
                    {
                        orderService.DeleteOrder(orderId);
                        Console.WriteLine("Order deleted!");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error deleting order: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine("Order deletion canceled.");
                }
            }
        }
        static void SearchCustomerByName(IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var customerService = scope.ServiceProvider.GetRequiredService<ICustomerService>();

                Console.WriteLine("\nSearch for a Customer");
                Console.WriteLine("----------------------");
                Console.Write("Enter customer name (or part of it): ");
                string inputName = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(inputName))
                {
                    Console.WriteLine("Name cannot be empty.");
                    return;
                }

                var customers = customerService.GetAllCustomers()
                    .Where(c => c.Name.Contains(inputName, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (customers.Any())
                {
                    Console.WriteLine($"\nFound {customers.Count} customer(s):");
                    foreach (var customer in customers)
                    {
                        Console.WriteLine("----------------------");
                        Console.WriteLine($"ID: {customer.Id}");
                        Console.WriteLine($"Name: {customer.Name}");
                        Console.WriteLine($"Email: {customer.Email}");
                        Console.WriteLine($"Phone Number: {customer.PhoneNumber}");
                        Console.WriteLine("----------------------");
                    }
                }
                else
                {
                    Console.WriteLine("No customers found with the specified name.");
                }
            }
        }
        
    }
}
