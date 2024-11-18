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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CourseWorkTest
{
    /// <summary>
    /// Логика взаимодействия для AdminLoginPage.xaml
    /// </summary>
    public partial class AdminLoginPage : Window
    {
        public AdminLoginPage()
        {
            InitializeComponent();
        }

       

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            string username = loginTextBox.Text;
            string password = passwordTextBox.Password;
            string code = codeTextBox.Text;
            string loginData = $"admin:{username}:{password}:{code}";

            string response = await NetworkService.Instance.SendMessageAsync(loginData);

            if (response == "Success")
            {
                AdminPage adminPage = new AdminPage();
                adminPage.Show();
                this.Hide();
            }
            else
            {
                MessageBox.Show("Invalid credentials");
            }
        }

        private void Clear_MouseDown(object sender, MouseButtonEventArgs e)
        {
            loginTextBox.Text = "";
            passwordTextBox.Password = "";
            codeTextBox.Text = "";
            loginTextBox.Focus();
        }
    }
}
