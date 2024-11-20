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
using TESTINGCOURSEWORK;
using TESTINGCOURSEWORK.SupplierFolder;

namespace CourseWorkTest
{
    /// <summary>
    /// Логика взаимодействия для StartPage.xaml
    /// </summary>
    public partial class StartPage : Window
    {
        
        public StartPage()
        {
            InitializeComponent();
        }

       
        private void admin_Button_Click(object sender, RoutedEventArgs e)
        {
           AdminLoginPage adminLoginPage = new AdminLoginPage();
            adminLoginPage.Show();
            this.Hide();
        }

        private void Manager_Button_Click(object sender, RoutedEventArgs e)
        {
           MenegerLoginPage menegerLoginPage = new MenegerLoginPage();
            menegerLoginPage.Show();
            this.Hide();
        }

        private void client_Button_Click(object sender, RoutedEventArgs e)
        {
            SupplierRegistrationPage clientRegPage = new SupplierRegistrationPage();
            clientRegPage.Show();
            this.Hide();
        }

        private void Supplier_Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
