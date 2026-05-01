using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XA68SP_HSZF_2025261.Models;
using XA68SP_HSZF_2025261.Persistence.MsSql;

namespace XA68SP_HSZF_2025261.Application
{
    public class ProductService : IProductService
    {
        private readonly IProductDataProvider productDataProvider;
        private readonly IOrderDataProvider orderDataProvider;

        public ProductService(IProductDataProvider productDataProvider, IOrderDataProvider orderDataProvider)
        {
            this.productDataProvider = productDataProvider;
            this.orderDataProvider = orderDataProvider;
        }
        public void CreateProduct(Product product)
        {
            if (string.IsNullOrWhiteSpace(product.Name) || product.Price <= 0 || product.Stock < 0)
                throw new ArgumentException("Product details are invalid. Ensure all fields are properly set.");
            productDataProvider.CreateProduct(product);
        }
        public event EventHandler<string> ProductStockLow;
        public void UpdateProduct(Product product)
        {
            var existingProduct = productDataProvider.ReadAllProducts().FirstOrDefault(p => p.Id == product.Id);

            if (existingProduct == null)
                throw new InvalidOperationException($"Product with ID {product.Id} does not exist.");

            if (product.Stock < 10)
            {
                ProductStockLow?.Invoke(this, $"Product '{product.Name}' (ID: {product.Id}) is running low on stock.");
            }

            productDataProvider.UpdateProduct(product);
        }
        public void DeleteProduct(int productId)
        {
            var existingProduct = productDataProvider.ReadAllProducts().FirstOrDefault(p => p.Id == productId);

            if (existingProduct == null)
                throw new InvalidOperationException($"Product with ID {productId} does not exist.");

            productDataProvider.DeleteProduct(productId);
        }
        public IEnumerable<Product> GetAllProducts()
        {
            return productDataProvider.ReadAllProducts();
        }
        public void GenerateCategoryRevenueReports(string rootDirectory = "CategoryReports")
        {
            if (!Directory.Exists(rootDirectory))
            {
                Directory.CreateDirectory(rootDirectory);
            }

            var orders = orderDataProvider.ReadAllOrders();
            var products = productDataProvider.ReadAllProducts();

            var categoryRevenues = products
                .GroupBy(p => p.Category)
                .Select(group => new
                {
                    Category = group.Key,
                    Revenue = group.Sum(p =>
                        orders.Where(o => o.ProductId == p.Id).Sum(o => o.Quantity * p.Price))
                })
                .ToList();

            foreach (var category in categoryRevenues)
            {
                string categoryDirectory = Path.Combine(rootDirectory, category.Category);
                if (!Directory.Exists(categoryDirectory))
                {
                    Directory.CreateDirectory(categoryDirectory);
                }

                string fileName = $"Revenue_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                string filePath = Path.Combine(categoryDirectory, fileName);

                var reportLines = new List<string>
                {
                    $"Category Revenue Report for {category.Category}:",
                    $"Total Revenue: {category.Revenue:C}",
                };

                File.WriteAllLines(filePath, reportLines);
            }
        }
        public void ExportLowStockProductsToJson(string directory = "LowStockReports")
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var lowStockProducts = productDataProvider.ReadAllProducts()
                .Where(p => p.Stock < 10)
                .Select(p => new
                {
                    Name = p.Name,
                    Price = p.Price,
                    Stock = p.Stock
                })
                .ToList();

            if (!lowStockProducts.Any())
            {
                Console.WriteLine("No products with stock below 10.");
                return;
            }

            string fileName = $"LowStockProducts_{DateTime.Now:yyyyMMdd_HHmmss}.json";
            string filePath = Path.Combine(directory, fileName);

            var jsonContent = JsonConvert.SerializeObject(lowStockProducts, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(filePath, jsonContent);

        }
    }
}
