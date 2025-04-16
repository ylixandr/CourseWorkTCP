using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using TCPServer;

namespace TESTINGCOURSEWORK.ManagerFolder
{
    /// <summary>
    /// Логика взаимодействия для AdjustStockWindow.xaml
    /// </summary>
    public partial class AdjustStockWindow : Window
    {
        public ObservableCollection<Product> Products { get; set; }

        public AdjustStockWindow(ObservableCollection<Product> products)
        {
            InitializeComponent();
            Products = products;
            ProductComboBox.ItemsSource = Products;
        }

        private async void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            //// Валидация данных
            //if (ProductComboBox.SelectedItem == null || string.IsNullOrWhiteSpace(QuantityTextBox.Text) || OperationTypeComboBox.SelectedItem == null)
            //{
            //    MessageBox.Show("Пожалуйста, заполните все поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            //    return;
            //}

            //if (!decimal.TryParse(QuantityTextBox.Text, out var quantity) || quantity <= 0)
            //{
            //    MessageBox.Show("Некорректное значение количества.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            //    return;
            //}

            //var selectedProduct = (Product)ProductComboBox.SelectedItem;
            //var transactionType = ((ComboBoxItem)OperationTypeComboBox.SelectedItem).Tag.ToString()!;
            //var description = DescriptionTextBox.Text;

            //// Создаем запись в Descriptions
            //int? descriptionId = null;
            //if (!string.IsNullOrWhiteSpace(description))
            //{
            //    using (var context = new CrmsystemContext())
            //    {
            //        var descriptionEntity = new Description
            //        {
            //            Content = description
            //        };
            //        context.Descriptions.Add(descriptionEntity);
            //        await context.SaveChangesAsync();
            //        descriptionId = descriptionEntity.Id;
            //    }
            //}

            //// Формирование данных для запроса
            //var request = new StockAdjustmentRequest
            //{
            //    ProductId = selectedProduct.ProductId,
            //    Quantity = transactionType == "Приход" ? quantity : -quantity,
            //    TransactionType = transactionType,
            //    DescriptionId = descriptionId // Используем DescriptionId вместо Description
            //};

            //// Отправка на сервер
            //string command = "adjustStock";
            //string jsonData = JsonConvert.SerializeObject(request);

            //string response = await NetworkService.Instance.SendMessageAsync($"{command}:{jsonData}");

            //// Обработка ответа
            //if (response == "Success")
            //{
            //    MessageBox.Show("Операция успешно выполнена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            //    ManagerPage managerPage = new ManagerPage();
            //    managerPage.Show();
            //    this.Close();
            //}
            //else
            //{
            //    MessageBox.Show("Ошибка при выполнении операции.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            //}
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            ManagerPage managerPage = new ManagerPage();
            managerPage.Show();

            this.Close();
        }
    }
}
