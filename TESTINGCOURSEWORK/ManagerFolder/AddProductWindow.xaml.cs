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

namespace TESTINGCOURSEWORK.ManagerFolder
{
    /// <summary>
    /// Логика взаимодействия для AddProductWindow.xaml
    /// </summary>
    public partial class AddProductWindow : Window
    {
        public AddProductWindow()
        {
            InitializeComponent();
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Проверка заполнения полей
                string productName = ProductNameTextBox.Text;
                string unitOfMeasurement = UnitOfMeasurementTextBox.Text;
                if (!decimal.TryParse(QuantityTextBox.Text, out decimal quantity) ||
                    !decimal.TryParse(UnitPriceTextBox.Text, out decimal unitPrice))
                {
                    MessageBox.Show("Введите корректные значения для количества и цены!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Формируем сообщение для сервера
                string message = $"addProduct:{productName}:{unitOfMeasurement}:{quantity}:{unitPrice}";

                // Отправка данных на сервер
                var response = await NetworkService.Instance.SendMessageAsync(message);
                if (response == "Success")
                {
                    MessageBox.Show("Продукт успешно добавлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    ManagerPage managerPage = new ManagerPage();
                    managerPage.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Ошибка при добавлении продукта.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            ManagerPage managerPage = new ManagerPage();
            managerPage.Show();
            this.Close();

        }
    }
}
