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
    /// <summary>
    /// Interaction logic for AddProductWindow.xaml
    /// </summary>
    public partial class AddProductWindow : Window
    {
        public AddProductWindow()
        {
            InitializeComponent();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var productService = App.ServiceProvider.GetService<IProductService>();
            var product = new Product
            {
                Name = NameBox.Text,
                Brand = BrandBox.Text,
                Category = CategoryBox.Text,
                Description = DescriptionBox.Text,
                Price = decimal.Parse(PriceBox.Text),
                Stock = int.Parse(StockBox.Text)
            };
            productService.CreateProduct(product);
            this.Close();
        }
    }
}
