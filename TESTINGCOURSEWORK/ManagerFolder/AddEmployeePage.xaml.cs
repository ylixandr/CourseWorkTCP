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
    /// Логика взаимодействия для AddEmployeePage.xaml
    /// </summary>
    public partial class AddEmployeePage : Window
    {
        public AddEmployeePage()
        {
            InitializeComponent();
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string firstName = FirstNameTextBox.Text;
                string lastName = LastNameTextBox.Text;
                string middleName = MiddleNameTextBox.Text;
                decimal salary = decimal.Parse(SalaryTextBox.Text);
                DateTime birthDate = BirthDatePicker.SelectedDate ?? throw new Exception("Выберите дату рождения.");
                string position = PositionTextBox.Text;
                DateTime hireDate = HireDatePicker.SelectedDate ?? throw new Exception("Выберите дату найма.");

                string message = $"addEmployee:{firstName}:{lastName}:{middleName}:{salary}:{birthDate:yyyy-MM-dd}:{position}:{hireDate:yyyy-MM-dd}";

                var response = await NetworkService.Instance.SendMessageAsync(message);
                if (response == "Success")
                {
                    MessageBox.Show("Сотрудник успешно добавлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    ManagerPage managerPage = new ManagerPage();
                    managerPage.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Ошибка при добавлении сотрудника.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
            this.Hide();
        }
    }
}

