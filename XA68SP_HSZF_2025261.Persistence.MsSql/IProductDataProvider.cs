using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XA68SP_HSZF_2025261.Models;

namespace XA68SP_HSZF_2025261.Persistence.MsSql
{
    public interface IProductDataProvider
    {
        void CreateProduct(Product product);
        void DeleteProduct(int productId);
        IEnumerable<Product> ReadAllProducts();
        void UpdateProduct(Product product);
    }
}
