using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
    /// <summary>
    /// Логика взаимодействия для ManageDeductionRatesWindow.xaml
    /// </summary>
    public partial class ManageDeductionRatesWindow : Window
    {
        private readonly Dictionary<string, decimal> _deductionRates;
        private readonly ObservableCollection<DeductionRate> _rates;

        public ManageDeductionRatesWindow(Dictionary<string, decimal> deductionRates)
        {
            InitializeComponent();
            _deductionRates = deductionRates;
            _rates = new ObservableCollection<DeductionRate>(
                deductionRates.Select(kv => new DeductionRate { Type = kv.Key, Rate = kv.Value * 100 }));
            RatesListView.ItemsSource = _rates;
        }

        private void SaveRates_Click(object sender, RoutedEventArgs e)
        {
            var updatedRates = _rates.ToDictionary(r => r.Type, r => r.Rate / 100m);
            File.WriteAllText("DeductionRates.json", JsonConvert.SerializeObject(updatedRates, Formatting.Indented));
            foreach (var rate in updatedRates)
            {
                _deductionRates[rate.Key] = rate.Value;
            }
            MessageBox.Show("Ставки успешно сохранены.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            Close();
        }
    }

    public class DeductionRate
    {
        public string Type { get; set; }
        public decimal Rate { get; set; }
    }
}