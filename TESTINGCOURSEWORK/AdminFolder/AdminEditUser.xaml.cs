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
    /// Логика взаимодействия для AdminEditUser.xaml
    /// </summary>
    public partial class AdminEditUser : Window
    {
        public AdminEditUser()
        {
            InitializeComponent();
        }

        private void Back_Button_Click(object sender, RoutedEventArgs e)
        {
            AdminPage adminPage = new AdminPage();
            adminPage.Show();
            this.Hide();
        }
    }
}
