
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

namespace TESTINGCOURSEWORK
{
    /// <summary>
    /// Логика взаимодействия для MenegerLoginPage.xaml
    /// </summary>
    public partial class MenegerLoginPage : Window
    {
        public MenegerLoginPage()
        {
            InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            string username = loginTextBox.Text;
            string password = passwordTextBox.Password;
            string code = codeTextBox.Text;
            string loginData = $"manager:{username}:{password}:{code}";

            string response = await NetworkService.Instance.SendMessageAsync(loginData);

            if (response == "Success")
            {
                ManagerPage managerPage = new ManagerPage();
                managerPage.Show();
                this.Hide();
            }
            else
            {
                MessageBox.Show("Invalid credentials");
            }
        }
        private void Admin_click(object sender, MouseButtonEventArgs e)
        {
            AdminLoginPage loginPage = new AdminLoginPage();    
            loginPage.Show();
            this.Hide();
        }
        private void Clear_MouseDown(object sender, MouseButtonEventArgs e)
        {
            loginTextBox.Text = "";
            passwordTextBox.Password = "";
            codeTextBox.Text = "";
        }
    }
}
