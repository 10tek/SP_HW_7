using HW.Domain;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HW
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Func<List<Product>> funcUpdProducts;
        private Action actionUpdCategories;
        private Action<string> actionSaveCategory;
        private Action<Product> actionSaveProduct;
        private List<Category> categories = new List<Category>();
        private bool isProduct = false;
        private Category selectedCategory;
        private Product selectedProduct;

        public MainWindow()
        {
            InitializeComponent();
            funcUpdProducts = new Func<List<Product>>(UpdateProductsLB);
            actionUpdCategories = new Action(UpdateCategoriesLB);
            actionSaveCategory = new Action<string>(SaveOrUpdateCategory);
            actionSaveProduct = new Action<Product>(SaveOrUpdateProduct);
            actionUpdCategories.BeginInvoke(null, null);
            categoriesCB.ItemsSource = categories;
            categoriesLB.ItemsSource = categories;
        }

        //метод для action
        private void UpdateCategoriesLB()
        {
            using (var context = new HwContext())
            {
                categories = context.Categories.ToList();
            }
        }

        private void CloseAll()
        {
            infoL.Content = "Выберите категорию, либо создайте её";
            categoriesCB.Visibility = Visibility.Hidden;
            categoriesL.Visibility = Visibility.Hidden;
            nameL.Visibility = Visibility.Hidden;
            nameTB.Visibility = Visibility.Hidden;
            saveBtn.Visibility = Visibility.Hidden;
            productsLB.Visibility = Visibility.Hidden;
            selectedProduct = null;
            selectedCategory = null;
        }

        private void ShowCategory()
        {
            nameL.Visibility = Visibility.Visible;
            nameTB.Visibility = Visibility.Visible;
            saveBtn.Visibility = Visibility.Visible;
        }

        private void ShowProduct()
        {
            ShowCategory();
            categoriesCB.Visibility = Visibility.Visible;
            categoriesL.Visibility = Visibility.Visible;
        }

        private void CategoriesBtnClick(object sender, RoutedEventArgs e)
        {
            infoL.Content = "Выберите категорию, либо создайте её. Для просмотра продуктов выберите категорию, и нажмите кнопку \"продукты\"";
            categoriesLB.Visibility = Visibility.Visible;
            actionUpdCategories.BeginInvoke(null, null);
            CloseAll();
            categoriesCB.ItemsSource = categories;
            categoriesLB.ItemsSource = categories;
            isProduct = false;
        }

        private void CreateBtnClick(object sender, RoutedEventArgs e)
        {
            if (isProduct)
            {
                categoriesCB.Visibility = Visibility.Visible;
                categoriesL.Visibility = Visibility.Visible;
                categoriesCB.Text = selectedCategory.Name;
                infoL.Content = "Создание продукта, пожалуйста заполните данные!";
            }
            else
            {
                categoriesCB.Visibility = Visibility.Hidden;
                categoriesL.Visibility = Visibility.Hidden;
                infoL.Content = "Создание категория, пожалуйста заполните данные!";
            }
            nameL.Visibility = Visibility.Visible;
            nameTB.Visibility = Visibility.Visible;
            nameTB.Text = string.Empty;
            saveBtn.Visibility = Visibility.Visible;
        }

        private void SaveBtnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(nameTB.Text))
            {
                MessageBox.Show("Введите наименование!");
                return;
            }
            selectedCategory = categoriesLB.SelectedItem as Category;
            if (isProduct)
                actionSaveProduct.BeginInvoke(new Product
                {
                    Category = categories[categoriesCB.SelectedIndex],
                    Name = nameTB.Text
                }, null, null);
            else
            {
                actionSaveCategory.BeginInvoke(nameTB.Text, null, null);

                categoriesLB.ItemsSource = null;
                categoriesCB.ItemsSource = null;
                categoriesCB.ItemsSource = categories;
                categoriesLB.ItemsSource = categories;
            }
            CloseAll();

        }

        //метод для action
        private void SaveOrUpdateProduct(Product product)
        {
            using (var context = new HwContext())
            {
                if (selectedProduct != null)
                {
                    selectedProduct = context.Products.FirstOrDefault(x => x.Id == selectedProduct.Id);
                    selectedProduct.Name = product.Name;
                    selectedProduct.CategoryId = product.Category.Id;
                    context.SaveChanges();
                    return;
                }
                context.Products.Add(product);
                context.SaveChanges();
            }
        }

        //метод для action
        private void SaveOrUpdateCategory(string name)
        {
            using (var context = new HwContext())
            {
                if (selectedCategory != null)
                {
                    try
                    {
                        var contextCategory = context.Categories.FirstOrDefault(x => x.Id == selectedCategory.Id);
                        contextCategory.Name = name;
                        context.SaveChanges();
                        return;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
                context.Categories.Add(new Category
                {
                    Name = name
                });
                context.SaveChanges();
                categories = context.Categories.ToList();

            }
        }

        private void CategoriesLBSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (categoriesLB.SelectedIndex == -1) return;
            selectedCategory = categoriesLB.SelectedItem as Category;
            ShowCategory();
            nameTB.Text = selectedCategory.ToString();
            isProduct = false;
        }

        private void ProductsBtnClick(object sender, RoutedEventArgs e)
        {
            infoL.Content = "Для просмотра продуктов выберите категорию, и нажмите кнопку \"продукты\"";
            selectedCategory = categoriesLB.SelectedItem as Category;
            if (selectedCategory is null) return;
            nameTB.Visibility = Visibility.Hidden;
            nameL.Visibility = Visibility.Hidden;
            infoL.Content = $"Продукты категории {selectedCategory.Name}";
            isProduct = true;
            categoriesLB.Visibility = Visibility.Hidden;
            productsLB.Visibility = Visibility.Visible;
            productsLB.ItemsSource = null;
            var asyncResult = funcUpdProducts.BeginInvoke(null, null);
            productsLB.ItemsSource = funcUpdProducts.EndInvoke(asyncResult);
        }

        //метод для Func
        private List<Product> UpdateProductsLB()
        {
            using (var context = new HwContext())
            {
                selectedCategory = context.Categories.FirstOrDefault(x => x.Id == selectedCategory.Id);
                var products = context.Products.Where(x => x.CategoryId == selectedCategory.Id).ToList();
                return products;
            }
        }

        private void ProductsLBSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (productsLB.SelectedIndex == -1) return;
            selectedProduct = productsLB.SelectedItem as Product;
            ShowProduct();
            categoriesCB.Text = selectedCategory.Name;
            nameTB.Text = selectedProduct.Name;
            isProduct = true;
        }
    }
}
