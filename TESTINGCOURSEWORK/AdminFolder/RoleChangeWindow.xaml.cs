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

namespace TESTINGCOURSEWORK.AdminFolder
{
    /// <summary>
    /// Логика взаимодействия для RoleChangeWindow.xaml
    /// </summary>
    public partial class RoleChangeWindow : Window
    {
        public int? SelectedRole { get; private set; }

        public RoleChangeWindow()
        {
            InitializeComponent();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (RoleComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                SelectedRole = int.Parse(selectedItem.Content.ToString());
                DialogResult = true; // Закрыть окно с подтверждением
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите роль.");
            }
        }
    }
}
