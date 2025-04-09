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

namespace Client.ManagerFolder
{
    public partial class ManageTypesWindow : Window
    {
        public ObservableCollection<string> Types { get; }
        private readonly Action<string> _addType;
        private readonly Action<string> _removeType;

        public ManageTypesWindow(ObservableCollection<string> types, Action<string> addType, Action<string> removeType)
        {
            InitializeComponent();
            Types = types;
            _addType = addType;
            _removeType = removeType;
            TypesListBox.ItemsSource = Types;
        }

        private void AddType_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NewTypeTextBox.Text))
            {
                MessageBox.Show("Введите название типа.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _addType(NewTypeTextBox.Text);
            NewTypeTextBox.Text = string.Empty;
        }

        private void RemoveType_Click(object sender, RoutedEventArgs e)
        {
            if (TypesListBox.SelectedItem is string selectedType)
            {
                _removeType(selectedType);
            }
            else
            {
                MessageBox.Show("Выберите тип для удаления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
