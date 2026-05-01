using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;
using System.Xml.Linq;
using XA68SP_HSZF_2025261.Application;
using XA68SP_HSZF_2025261.Models;
using XA68SP_HSZF_2025261.Persistence.MsSql;

namespace XA68SP_HSZF_2025261.WPFClient
{
    // Az alkalmazás belépési pontja.
    // Itt történik az alkalmazás inicializálása.
    //
    // Itt állítjuk be a szükséges szolgáltatásokat (service-ek),
    // valamint innen indul a főablak (MainWindow).
    //
    // Az adatok betöltése (XML vagy adatbázis) is itt történik induláskor.
    public partial class App : System.Windows.Application
    {
        public static ServiceProvider ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            var services = new ServiceCollection();

            ConfigureServices(services);

            ServiceProvider = services.BuildServiceProvider();
            LoadData();

            var mainWindow = ServiceProvider.GetService<MainWindow>();
            mainWindow.Show();
        }

        private void ConfigureServices(ServiceCollection services)
        {
            services.AddScoped<IProductDataProvider, ProductDataProvider>();
            services.AddScoped<IOrderDataProvider, OrderDataProvider>();
            services.AddScoped<ICustomerDataProvider, CustomerDataProvider>();

            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<ICustomerService, CustomerService>();

            services.AddSingleton<MainViewModel>();

            services.AddSingleton<MainWindow>();
            services.AddTransient<AddProductWindow>();

            services.AddDbContext<ShopDbContext>();
        }

        private void LoadData()
        {
            string filePath = "shop.xml";

            if (!File.Exists(filePath))
                return;

            var customerService = ServiceProvider.GetRequiredService<ICustomerService>();
            var productService = ServiceProvider.GetRequiredService<IProductService>();
            var orderService = ServiceProvider.GetRequiredService<IOrderService>();

            var xmlContent = File.ReadAllText(filePath);
            var xDoc = XDocument.Parse(xmlContent);

            foreach (var p in xDoc.Descendants("Product"))
            {
                var product = new Product
                {
                    Id = int.Parse(p.Element("Id").Value),
                    Name = p.Element("Name").Value,
                    Brand = p.Element("Brand").Value,
                    Category = p.Element("Category").Value,
                    Price = decimal.Parse(p.Element("Price").Value),
                    Stock = int.Parse(p.Element("Stock").Value),
                    Description = p.Element("Description").Value
                };

                if (!productService.GetAllProducts().Any(x => x.Id == product.Id))
                {
                    productService.CreateProduct(product);
                }
            }

            foreach (var c in xDoc.Descendants("Customer"))
            {
                var customer = new Customer
                {
                    Id = int.Parse(c.Element("Id").Value),
                    Name = c.Element("Name").Value,
                    Email = c.Element("Email").Value,
                    PhoneNumber = c.Element("PhoneNumber").Value
                };

                if (!customerService.GetAllCustomers().Any(x => x.Id == customer.Id))
                {
                    customerService.CreateCustomer(customer);
                }
            }

            foreach (var o in xDoc.Descendants("Order"))
            {
                var order = new Order
                {
                    Id = int.Parse(o.Element("Id").Value),
                    ProductId = int.Parse(o.Element("ProductId").Value),
                    CustomerId = int.Parse(o.Element("CustomerId").Value),
                    Quantity = int.Parse(o.Element("Quantity").Value),
                    OrderDate = DateTime.Parse(o.Element("OrderDate").Value)
                };

                if (!orderService.GetAllOrders().Any(x => x.Id == order.Id))
                {
                    orderService.CreateOrder(order);
                }
            }
        }
    }
}
