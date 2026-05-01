using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XA68SP_HSZF_2025261.Models;

namespace XA68SP_HSZF_2025261.Persistence.MsSql
{
    public class ProductDataProvider : IProductDataProvider
    {
        private readonly ShopDbContext dbContext;

        public ProductDataProvider(ShopDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public void CreateProduct(Product product)
        {
            dbContext.Products.Add(product);
            dbContext.SaveChanges();
        }
        public void DeleteProduct(int productId)
        {
            var productToDelete = dbContext.Products.FirstOrDefault(p => p.Id == productId);
            if (productToDelete != null)
            {
                dbContext.Products.Remove(productToDelete);
                dbContext.SaveChanges();
            }
        }
        public IEnumerable<Product> ReadAllProducts()
        {
            return dbContext.Products.ToList();
        }
        public void UpdateProduct(Product product)
        {
            var productToUpdate = dbContext.Products.FirstOrDefault(p => p.Id == product.Id);
            if (productToUpdate != null)
            {
                productToUpdate.Name = product.Name;
                productToUpdate.Brand = product.Brand;
                productToUpdate.Category = product.Category;
                productToUpdate.Price = product.Price;
                productToUpdate.Stock = product.Stock;
                productToUpdate.Description = product.Description;
                dbContext.SaveChanges();
            }
        }
    }
}
