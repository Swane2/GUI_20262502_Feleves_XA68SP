using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using XA68SP_HSZF_2025261.Application;
using XA68SP_HSZF_2025261.Models;

namespace XA68SP_HSZF_2025261.WPFClient
{
    public partial class EditProductWindow : Window
    {
        private readonly Product product;

        public EditProductWindow(Product product)
        {
            InitializeComponent();
            this.product = product;

            NameBox.Text = product.Name;
            BrandBox.Text = product.Brand;
            CategoryBox.Text = product.Category;
            DescriptionBox.Text = product.Description;
            PriceBox.Text = product.Price.ToString();
            StockBox.Text = product.Stock.ToString();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var productService = App.ServiceProvider.GetService<IProductService>();

            product.Name = NameBox.Text;
            product.Brand = BrandBox.Text;
            product.Category = CategoryBox.Text;
            product.Description = DescriptionBox.Text;
            product.Price = decimal.Parse(PriceBox.Text);
            product.Stock = int.Parse(StockBox.Text);

            productService.UpdateProduct(product);
            this.Close();
        }
    }
}
