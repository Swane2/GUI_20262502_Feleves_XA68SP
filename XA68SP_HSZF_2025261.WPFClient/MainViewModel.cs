using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using XA68SP_HSZF_2025261.Application;
using XA68SP_HSZF_2025261.Models;

namespace XA68SP_HSZF_2025261.WPFClient
{
    public class MainViewModel
    {
        private readonly IProductService productService;

        public ObservableCollection<Product> Products { get; set; }

        public IRelayCommand AddProductCommand { get; }
        public IRelayCommand DeleteProductCommand { get; }
        public IRelayCommand ShowDetailsCommand { get; }
        public IRelayCommand EditProductCommand { get; }

        public MainViewModel(IProductService productService)
        {
            this.productService = productService;

            Products = new ObservableCollection<Product>(productService.GetAllProducts());

            AddProductCommand = new RelayCommand(OpenAddWindow);
            DeleteProductCommand = new RelayCommand<object>(
                    execute: param => { if (param is Product p) DeleteProduct(p); },
                    canExecute: param => param is Product);
            ShowDetailsCommand = new RelayCommand<object>(
                    execute: param => { if (param is Product p) OpenDetailsWindow(p); },
                    canExecute: param => param is Product);
            EditProductCommand = new RelayCommand<object>(
                    execute: param => { if (param is Product p) OpenEditWindow(p); },
                    canExecute: param => param is Product);
        }

        private void OpenAddWindow()
        {
            var window = App.ServiceProvider.GetService<AddProductWindow>();
            window.ShowDialog();

            Refresh();
        }

        private void DeleteProduct(Product product)
        {
            productService.DeleteProduct(product.Id);
            Refresh();
        }

        private void Refresh()
        {
            Products.Clear();
            foreach (var p in productService.GetAllProducts())
                Products.Add(p);
        }

        private void OpenDetailsWindow(Product product)
        {
            var window = new ProductDetailsWindow(product);
            window.ShowDialog();
        }
        private void OpenEditWindow(Product product)
        {
            var window = new EditProductWindow(product);
            window.ShowDialog();
            Refresh();
        }
    }
}
