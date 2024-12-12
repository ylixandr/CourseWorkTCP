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
using TESTINGCOURSEWORK.AdminFolder;
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
        private async void ChangeRoleMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Получение выбранного пользователя
            if (UserDataGrid.SelectedItem is TESTINGCOURSEWORK.Models.Account selectedAccount)
            {
                var roleChangeWindow = new RoleChangeWindow();
                if (roleChangeWindow.ShowDialog() == true && roleChangeWindow.SelectedRole.HasValue)
                {
                    int newRoleId = roleChangeWindow.SelectedRole.Value;

                    // Отправка запроса на сервер
                    try
                    {
                        string request = $"updateRole:{selectedAccount.Id}:{newRoleId}";
                        string response = await NetworkService.Instance.SendMessageAsync(request);

                        if (response == "Success")
                        {
                            MessageBox.Show("Роль успешно обновлена!");

                            // Обновление данных DataGrid
                            selectedAccount.RoleId = newRoleId;
                            UserDataGrid.Items.Refresh(); // Перерисовка таблицы
                        }
                        else
                        {
                            MessageBox.Show("Ошибка при обновлении роли.");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка: {ex.Message}");
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите пользователя.");
            }
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

        private void panel_ButtonClick(object sender, RoutedEventArgs e)
        {
            topEditingPanel.Visibility = Visibility.Hidden;
            UserDataGrid.Visibility = Visibility.Hidden;
            EditAdminPanelWindow editAdminPanelWindow = new EditAdminPanelWindow();
            editAdminPanelWindow.ShowDialog();
        }

        private void Balance_Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Exit_Button_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}