using CourseWorkTest;
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
    /// Логика взаимодействия для SupplierRegistrationPage.xaml
    /// </summary>
    public partial class SupplierRegistrationPage : Window
    {
        public SupplierRegistrationPage()
        {
            InitializeComponent();
        }
        private void Label_MouseDown(object sender, MouseButtonEventArgs e)
        {
            LoginTextBox.Text = "";
            passwordTextBox.Password = "";
            passwordTextBox_repeated.Password = "";
            LoginTextBox.Focus();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (LoginTextBox.Text == "" && passwordTextBox_repeated.Password == "" && passwordTextBox.Password == "")
            {
                MessageBox.Show("Неправильно введены данные", "Sign Up Failed", MessageBoxButton.OK);
            }
            else if (passwordTextBox.Password == passwordTextBox_repeated.Password)
            {
                string username = LoginTextBox.Text;
                string password = passwordTextBox.Password;

                string loginData = $"regUser:{username}:{password}";

                string response = await NetworkService.Instance.SendMessageAsync(loginData);
                if (response == "Success")
                {
                    AdminPage adminPage = new AdminPage();
                    adminPage.Show();
                    this.Hide();
                }
                else
                {
                    MessageBox.Show("Пользователь уже существует");
                }
            }
            else
            {
                MessageBox.Show("Попробуйте еще раз", "Registration Failed", MessageBoxButton.OK);
                passwordTextBox_repeated.Password = "";
                passwordTextBox.Password = "";
                passwordTextBox.Focus();

            }
        }

        private void Account_Alreary_Exists_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SupplierLoginPage loginPage = new SupplierLoginPage();
            loginPage.Show();
            this.Hide();
        }
    }
}
