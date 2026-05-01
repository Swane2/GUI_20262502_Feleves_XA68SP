using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XA68SP_HSZF_2025261.Models;

namespace XA68SP_HSZF_2025261.Application
{
    public interface IProductService
    {
        void CreateProduct(Product product);
        void UpdateProduct(Product product);
        void DeleteProduct(int productId);
        IEnumerable<Product> GetAllProducts();
        void GenerateCategoryRevenueReports(string rootDirectory = "CategoryReports");
        void ExportLowStockProductsToJson(string directory = "LowStockReports");
        event EventHandler<string> ProductStockLow;
    }
}
