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
using TCPServer;

namespace TESTINGCOURSEWORK.AdminFolder
{
    /// <summary>
    /// Логика взаимодействия для EditAdminPanelWindow.xaml
    /// </summary>
    public partial class EditAdminPanelWindow : Window
    {
        public EditAdminPanelWindow()
        {
            InitializeComponent();
            
        }

        private void RoleRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (AdminCodeTextBox == null || ManagerCodeTextBox == null)
                return;

            if (AdminRadioButton.IsChecked == true)
            {
                AdminCodeTextBox.IsEnabled = true;
                ManagerCodeTextBox.IsEnabled = false;
            }
            else if (ManagerRadioButton.IsChecked == true)
            {
                AdminCodeTextBox.IsEnabled = false;
                ManagerCodeTextBox.IsEnabled = true;
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверка, какая роль выбрана и ввод кода
            string adminCode = AdminCodeTextBox.Text.Trim();
            string managerCode = ManagerCodeTextBox.Text.Trim();

            if (AdminRadioButton.IsChecked == true && string.IsNullOrWhiteSpace(adminCode))
            {
                MessageBox.Show("Введите код администратора.");
                return;
            }

            if (ManagerRadioButton.IsChecked == true && string.IsNullOrWhiteSpace(managerCode))
            {
                MessageBox.Show("Введите код менеджера.");
                return;
            }

            // Формирование данных для отправки
            var adminPanelData = new AdminPanel
            {
                Id = 1, // Предположим, что редактируется объект с ID 1
                AdminCode = adminCode,
                ManagerCode = managerCode
            };

            try
            {
                // Отправка данных на сервер
                string request = $"updateAdminPanel:{managerCode}:{adminCode}";
                string response = await NetworkService.Instance.SendMessageAsync(request);

                if (response == "Success")
                {
                    MessageBox.Show("Данные успешно сохранены.");
                    this.Close();
                }
                else
                {
                    MessageBox.Show($"Ошибка: {response}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }
    }
}

