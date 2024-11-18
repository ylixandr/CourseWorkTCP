using Newtonsoft.Json;
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
    /// Логика взаимодействия для AddTransactionPage.xaml
    /// </summary>
    public partial class AddTransactionPage : Window
    {
        public AddTransactionPage()
        {
            InitializeComponent();
        }

        private async void AddTransactionButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Проверка данных
                if (TransactionTypeComboBox.SelectedItem == null ||
                    !decimal.TryParse(AmountTextBox.Text, out var amount) ||
                    string.IsNullOrWhiteSpace(DescriptionTextBox.Text))
                {
                    MessageBox.Show("Пожалуйста, заполните все поля корректно.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string transactionType = (TransactionTypeComboBox.SelectedItem as ComboBoxItem).Content.ToString();
                string description = DescriptionTextBox.Text;

                // Формат команды для сервера
                string command = $"transaction:{transactionType}:{amount}:{description}";

                // Отправка данных на сервер
                string response = await NetworkService.Instance.SendMessageAsync(command);

                // Обработка ответа
                if (response == "Success")
                {
                    MessageBox.Show("Транзакция успешно добавлена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    ManagerPage managerPage = new ManagerPage();
                    managerPage.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Ошибка при добавлении транзакции. Попробуйте снова.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


    }
}
