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
using TESTINGCOURSEWORK;
using TESTINGCOURSEWORK.Models;

namespace CourseWorkTest
{
    /// <summary>
    /// Логика взаимодействия для AdminPage.xaml
    /// </summary>
    public partial class AdminPage : Window
    {
        private ObservableCollection<Account> accounts;
        public ObservableCollection<Account> Accounts
        {
            get { return accounts; }
            set { accounts = value; }
        }
        public AdminPage()
        {
            InitializeComponent();


        }

        private void exit_Label_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            AdminLoginPage startPage = new AdminLoginPage();
            startPage.Show();
            this.Hide();
        }







        private void Edit_Button_Click(object sender, RoutedEventArgs e)
        {
            AdminEditUser adminEditUser = new AdminEditUser();
            adminEditUser.Show();
            this.Hide();
        }

        private async void Delete_Button_Click(object sender, RoutedEventArgs e)
        {
            if (UserDataGrid.SelectedItem is Account selectedAccount)
            {
                // Формирование сообщения для сервера
                string loginData = $"deleteUser:{selectedAccount.Id}";

                // Отправка сообщения на сервер
                string response = await NetworkService.Instance.SendMessageAsync(loginData);

                // Проверка ответа сервера
                if (response == "User deleted")
                {
                    MessageBox.Show("Пользователь успешно удален.");
                    // Обновление отображения
                    (UserDataGrid.ItemsSource as ObservableCollection<Account>)?.Remove(selectedAccount);
                }
                else
                {
                    MessageBox.Show("Не удалось удалить пользователя.");
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите пользователя для удаления.");
            }
        }

        private void Add_Button_Click(object sender, RoutedEventArgs e)
        {
            AdminAddNewUser adminAddNewUser = new AdminAddNewUser();
            adminAddNewUser.Show();
            this.Hide();
        }

        private void Server_Label_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private async void Users_Button_Click(object sender, RoutedEventArgs e)
        {
            topEditingPanel.Visibility = Visibility.Visible;
            UserDataGrid.Visibility = Visibility.Visible;
            try
            {
                string response = await NetworkService.Instance.SendMessageAsync("getUsers");

                if (response == "NoData")
                {
                    MessageBox.Show("Нет данных для отображения.");
                    return;
                }
                else
                {
                    List<Account>? users = new List<Account>();
                    users = JsonConvert.DeserializeObject<List<Account>>(response);
                    Accounts = new ObservableCollection<Account>(users);
                    UserDataGrid.ItemsSource = Accounts;

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }
    }
}