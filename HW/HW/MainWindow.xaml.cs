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
        private List<Category> categories = new List<Category>();
        private bool isProduct = false;
        private Category selectedCategory;

        public MainWindow()
        {
            InitializeComponent();
            ConnectItemSources();
        }

        private void ConnectItemSources()
        {
            using (var context = new HwContext())
            {
                categories = context.Categories.ToList();
            }
            categoriesLB.ItemsSource = null;
            categoriesCB.ItemsSource = null;
            categoriesCB.ItemsSource = categories;
            categoriesLB.ItemsSource = categories;
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
        }

        private void CategoriesBtnClick(object sender, RoutedEventArgs e)
        {
            infoL.Content = "Выберите категорию, либо создайте её. Для просмотра продуктов выберите категорию, и нажмите кнопку \"продукты\"";
            productsLB.Visibility = Visibility.Hidden;
            categoriesLB.Visibility = Visibility.Visible;
            isProduct = false;
            ConnectItemSources();
            CloseAll();
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
            saveBtn.Visibility = Visibility.Visible;
        }

        private void SaveBtnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(nameTB.Text))
            {
                MessageBox.Show("Введите наименование!");
                return;
            }
            try
            {
                using (var context = new HwContext())
                {
                    if (isProduct)
                    {
                        if(context.Products.FirstOrDefault(x=>x.Name == nameTB.Text) != null)
                        {
                            MessageBox.Show("Такой продукт уже существует");
                            return;
                        }
                        if (categoriesCB.SelectedIndex == -1)
                        {
                            MessageBox.Show("Выберите категорию");
                            return;
                        }
                        var category = categories[categoriesCB.SelectedIndex];
                        context.Products.Add(new Product
                        {
                            CategoryId = category.Id,
                            Name = nameTB.Text
                        });
                    }
                    else
                    {
                        if (context.Categories.FirstOrDefault(x => x.Name == nameTB.Text) != null)
                        {
                            MessageBox.Show("Такая категория уже существует");
                            return;
                        }
                        context.Categories.Add(new Category
                        {
                            Name = nameTB.Text
                        });
                    }
                    context.SaveChanges();
                }
                nameTB.Text = string.Empty;
                ConnectItemSources();
                CloseAll();
                MessageBox.Show("Успешно создано!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка {ex.Message}");
            }
        }

        private void CategoriesLBSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (categoriesLB.SelectedIndex == -1) return;
            selectedCategory = categoriesLB.SelectedItem as Category;
            isProduct = false;
        }

        private void ProductsBtnClick(object sender, RoutedEventArgs e)
        {
            infoL.Content = "Для просмотра продуктов выберите категорию, и нажмите кнопку \"продукты\"";
            if (categoriesLB.SelectedIndex == -1) return;
            infoL.Content = $"Продукты категории {selectedCategory.Name}";
            isProduct = true;
            selectedCategory = categoriesLB.SelectedItem as Category;
            categoriesLB.Visibility = Visibility.Hidden;
            productsLB.Visibility = Visibility.Visible;
            using (var context = new HwContext())
            {
                selectedCategory = context.Categories.FirstOrDefault(x => x.Id == selectedCategory.Id);
                if (context.Products.Where(x => x.CategoryId != selectedCategory.Id).ToList().Count != 0)
                {
                    productsLB.ItemsSource = null;
                    var products = context.Products.Where(x => x.CategoryId == selectedCategory.Id).ToList();
                    productsLB.ItemsSource = products;
                }
            }
        }

    }
}
