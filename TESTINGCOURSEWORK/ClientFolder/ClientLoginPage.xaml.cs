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
    /// Логика взаимодействия для ClientLoginPage.xaml
    /// </summary>
    public partial class ClientLoginPage : Window
    {
        public ClientLoginPage()
        {
            InitializeComponent();
        }

        private void RegistrationLabel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ClientRegPage clientRegPage = new ClientRegPage();
            clientRegPage.Show();
            this.Hide();
        }

        //private async void Button_Click(object sender, RoutedEventArgs e)
        //{
        //    string username = loginTextBox.Text;
        //    string password = passwordTextBox.Password;

        //    string loginData = $"loginUser:{username}:{password}";

        //    string response = await NetworkService.Instance.SendMessageAsync(loginData);

        //    if (response == "Success")
        //    {
        //        AdminPage adminPage = new AdminPage();
        //        adminPage.Show();
        //        this.Hide();
        //    }
        //    else
        //    {
        //        MessageBox.Show("Invalid credentials");
        //    }
        //}

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            string username = loginTextBox.Text;
            string password = passwordTextBox.Password;

            string loginData = $"loginUser:{username}:{password}";

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
    }
}
