using Microsoft.VisualBasic.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Pkcs;
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
    /// Логика взаимодействия для CreateTicketWindow.xaml
    /// </summary>
    public partial class CreateTicketWindow : Window
    {
        public CreateTicketWindow()
        {
            InitializeComponent();
        }

        private async void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailTextBox.Text.Trim();
            string description = DescriptionTextBox.Text.Trim();

            // Проверяем валидность Email и заполнение полей
            if (string.IsNullOrEmpty(email))
            {
                MessageBox.Show("Пожалуйста, введите корректный Email.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(description))
            {
                MessageBox.Show("Пожалуйста, введите описание проблемы.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                string message = $"addSupport:{email}:{description}:{CurrentUser.UserId}";

                // Отправляем запрос на сервер
                var response = await NetworkService.Instance.SendMessageAsync(message);
                if (response == "Success")
                {
                    MessageBox.Show("Заявка успешно создана!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Ошибка при создании заявки.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

       
    }
}

