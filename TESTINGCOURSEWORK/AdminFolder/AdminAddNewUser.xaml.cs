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
    /// Логика взаимодействия для AdminAddNewUser.xaml
    /// </summary>
    public partial class AdminAddNewUser : Window
    {
        public AdminAddNewUser()
        {
            InitializeComponent();
        }

        private void Back_Button_Click(object sender, RoutedEventArgs e)
        {
            AdminPage adminPage = new AdminPage();
            adminPage.Show();
            this.Hide();
        }



        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            bool res = false;
            string username = loginTextBox.Text;
            string password = PasswordTextBox.Password;
            int roleId = 1;

            if (loginTextBox.Text == "" && PasswordTextBox.Password == "")
            {
                MessageBox.Show("Неправильно введены данные", "Sign Up Failed", MessageBoxButton.OK);
            }
            else
            {
                res = true;
            }
            
            if (RoleID.Text == "1" || RoleID.Text == "2" || RoleID.Text == "3")
            {
                res = true;
                roleId = int.Parse(RoleID.Text);
            }
            else
            {
                MessageBox.Show("Неправильно набрано ID");
            }


            if (res)
            {
                string loginData = $"regUserAdmin:{username}:{password}:{roleId}";

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
            
        }

    }
}
