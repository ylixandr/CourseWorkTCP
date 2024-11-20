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

namespace TESTINGCOURSEWORK.SupplierFolder
{
    /// <summary>
    /// Логика взаимодействия для CreateNewApplication.xaml
    /// </summary>
    public partial class CreateNewApplication : Window
    {
        public CreateNewApplication()
        {
            InitializeComponent();
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string login = LoginTextBox.Text.Trim(); // Получаем логин
                string contactInfo = ContactInfoTextBox.Text.Trim();
                string productName = ProductNameTextBox.Text.Trim();
                string productDescription = DescriptionTextBox.Text.Trim();

                if (!int.TryParse(QuantityTextBox.Text, out int quantity) || quantity <= 0)
                {
                    throw new Exception("Некорректное количество.");
                }

                string unitOfMeasurement = UnitOfMeasurementTextBox.Text.Trim();
                if (string.IsNullOrWhiteSpace(unitOfMeasurement))
                {
                    throw new Exception("Единица измерения не может быть пустой.");
                }

                if (!decimal.TryParse(TotalPriceTextBox.Text, out decimal totalPrice) || totalPrice <= 0)
                {
                    throw new Exception("Некорректная общая стоимость.");
                }

                if (string.IsNullOrWhiteSpace(login) ||
                    string.IsNullOrWhiteSpace(contactInfo) ||
                    string.IsNullOrWhiteSpace(productName) ||
                    string.IsNullOrWhiteSpace(productDescription))
                {
                    throw new Exception("Все поля должны быть заполнены.");
                }

                // Формируем сообщение для сервера
                string message = $"addApplication:{login}:{contactInfo}:{productName}:{productDescription}:{quantity}:{unitOfMeasurement}:{totalPrice}";

                // Отправляем запрос на сервер
                var response = await NetworkService.Instance.SendMessageAsync(message);
                if (response == "Success")
                {
                    MessageBox.Show("Заявка успешно создана!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    SupplierMainPage supplierMainPage = new SupplierMainPage();
                    supplierMainPage.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Ошибка при создании заявки.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }




        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            // Очистка текстовых полей
            LoginTextBox.Text = string.Empty;
            ContactInfoTextBox.Text = string.Empty;
            ProductNameTextBox.Text = string.Empty;
            DescriptionTextBox.Text = string.Empty;
            QuantityTextBox.Text = string.Empty;
            UnitOfMeasurementTextBox.Text = string.Empty;
            TotalPriceTextBox.Text = string.Empty;

            // Устанавливаем фокус на первое поле (например, на LoginTextBox)
            LoginTextBox.Focus();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            SupplierMainPage supplierMainPage = new SupplierMainPage();
            supplierMainPage.Show();
            this.Close();
        }
    }
}
